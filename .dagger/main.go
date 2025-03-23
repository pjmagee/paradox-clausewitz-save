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
	{os: "linux", rid: "linux-x64", address: "mcr.microsoft.com/dotnet/sdk:10.0-preview-trixie-slim"},
	{os: "linux", rid: "linux-arm64", address: "mcr.microsoft.com/dotnet/sdk:10.0-preview-trixie-slim"},
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

type Rid = string

const (
	LinuxX64   Rid = "linux-x64"
	LinuxArm64 Rid = "linux-arm64"
)

func (m *ParadoxClausewitzSav) PublishAot(
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
	rid Rid,
) *dagger.File {
	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview-trixie-slim").
		WithExec([]string{"dpkg", "--add-architecture", "arm64"}).
		WithExec([]string{"apt-get", "update"}).
		WithExec([]string{"apt-get", "install", "-y", "clang", "gcc-aarch64-linux-gnu", "llvm", "zlib1g-dev", "zlib1g-dev:arm64"}).
		WithExec([]string{"rm", "-rf", "/var/lib/apt/lists/*"}).
		WithMountedDirectory("/repo", repoDir).
		WithWorkdir("/repo/src/MageeSoft.Paradox.Clausewitz.Save.Cli").
		WithExec([]string{"dotnet", "publish", "-c", "Release", "-r", string(rid), "-o", "/repo/bin/Release/" + string(rid)}).
		File("/repo/bin/Release/" + string(rid) + "/paradox-clausewitz-sav")
}

func (m *ParadoxClausewitzSav) BuildNativeLinux(
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.Directory {
	return dag.Directory().
		WithFile("linux-x64/paradox-clausewitz-sav", m.PublishAot(repoDir, LinuxX64)).
		WithFile("linux-arm64/paradox-clausewitz-sav", m.PublishAot(repoDir, LinuxArm64))
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
