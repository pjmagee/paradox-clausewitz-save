package main

import (
	"context"
	"dagger/paradox-clausewitz-sav/internal/dagger"
)

type ParadoxClausewitzSav struct {
}

var matrix = []struct {
	os      string
	rid     string
	address string
}{
	{os: "linux", rid: "linux-x64", address: "mcr.microsoft.com/dotnet/sdk:10.0.100-preview.2-noble-aot-amd64"},
	{os: "linux", rid: "linux-arm64", address: "mcr.microsoft.com/dotnet/sdk:10.0.100-preview.2-noble-aot-arm64v8"},
	{os: "darwin", rid: "osx-x64", address: "sickcodes/docker-osx:auto"},
	{os: "darwin", rid: "osx-arm64", address: "sickcodes/docker-osx:auto"},
}

// Build the project
func (m *ParadoxClausewitzSav) Build(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.Container {

	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithWorkdir("/repo/src").
		WithExec([]string{"dotnet", "build"})
}

func (m *ParadoxClausewitzSav) Tool(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.Container {

	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithWorkdir("/repo/src/MageeSoft.Paradox.Clausewitz.Save.Cli").
		WithExec([]string{"dotnet", "publish", "-p:PackAsTool=true"})
}

func (m *ParadoxClausewitzSav) Publish(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.Container {

	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithWorkdir("/repo/src/MageeSoft.Paradox.Clausewitz.Save.Cli").
		WithExec([]string{"dotnet", "publish"})
}

func (m *ParadoxClausewitzSav) BuildNativeLinux(
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.Directory {

	releases := dag.Directory()

	for _, m := range matrix {
		if m.os == "linux" {
			ctr := dag.Container().
				From(m.address).
				Terminal().
				WithMountedDirectory("/repo", repoDir).
				WithWorkdir("/repo/src/MageeSoft.Paradox.Clausewitz.Save.Cli").
				WithExec([]string{"dotnet", "restore", "-r", m.rid}).
				WithExec([]string{"dotnet", "publish", "-c", "Release", "-r", m.rid, "-o", "/repo/bin/Release/" + m.rid})
			releases = releases.WithDirectory(m.rid, ctr.Directory("/repo/bin/Release/"+m.rid))
		}

	}

	return releases
}

func (m *ParadoxClausewitzSav) BuildNativeDarwin(
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.Directory {

	releases := dag.Directory()

	for _, m := range matrix {
		if m.os == "darwin" {
			ctr := dag.Container().
				From(m.address).
				WithEnvVariable("GENERATE_UNIQUE", "true").
				WithExec([]string{"bash", "-c", "curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 10.0.100-preview.2"}).
				WithExec([]string{"bash", "-c", "echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc && echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc"}).
				WithExec([]string{"bash", "-c", "source ~/.bashrc"}).
				WithMountedDirectory("/repo", repoDir).
				WithWorkdir("/repo/src/MageeSoft.Paradox.Clausewitz.Save.Cli").
				WithExec([]string{"dotnet", "restore", "-r", m.rid}).
				WithExec([]string{"dotnet", "publish", "-c", "Release", "-r", m.rid, "-o", "/repo/bin/Release/" + m.rid})
			releases = releases.WithDirectory(m.rid, ctr.Directory("/repo/bin/Release/"+m.rid))
		}
	}

	return releases
}

func (m *ParadoxClausewitzSav) VsTest(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) (string, error) {

	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithWorkdir("/repo/src").
		WithExec([]string{"dotnet", "run", "--project", "MageeSoft.Paradox.Clausewitz.Save.Tests"}).
		Stdout(ctx)
}

func (m *ParadoxClausewitzSav) Test(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) (string, error) {

	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithWorkdir("/repo/src").
		WithExec([]string{"dotnet", "test"}).
		Stdout(ctx)
}

func (m *ParadoxClausewitzSav) LinuxTest(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) (string, error) {

	savFile := dag.CurrentModule().Source().File("saves/stellaris/ironman.sav")

	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithMountedFile("/root/.paradoxlauncher/Stellaris/save games/my test empire/ironman.sav", savFile).
		WithWorkdir("/repo/src/MageeSoft.Paradox.Clausewitz.Save.Cli").
		WithExec([]string{"dotnet", "run", "--", "list"}).
		Stdout(ctx)
}
