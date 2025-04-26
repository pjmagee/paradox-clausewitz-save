package main

import (
	"context"
	"dagger/paradox-clausewitz-sav/internal/dagger"
	"strconv"
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
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview-aot").
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
		WithWorkdir("/repo/src/MageeSoft.PDX.CE.Cli").
		WithExec([]string{"dotnet", "publish", "-p:PackAsTool=true"})
}

// dotnet publish the MageeSoft.PDX.CE.Cli project and return the container.
func (m *ParadoxClausewitzSav) Publish(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
	// Whether to publish the project with AOT compilation
	// +defaultValue=false
	// +optional
	aot bool,
) *dagger.Container {
	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview-aot").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithWorkdir("/repo/src/MageeSoft.PDX.CE.Cli").
		WithExec([]string{"dotnet", "publish", "-c", "Release", "-p:PublishAot=" + strconv.FormatBool(aot)})
}

type Rid = string

const (
	LinuxX64   Rid = "linux-x64"
	LinuxArm64 Rid = "linux-arm64"
)

// dotnet publish the MageeSoft.PDX.CE.Cli project for the given rid and return the path to the binary.
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
		WithWorkdir("/repo/src/MageeSoft.PDX.CE.Cli").
		WithExec([]string{"dotnet", "publish", "-c", "Release", "-r", string(rid), "/p:PublishAot=true", "-o", "/repo/bin/Release/" + string(rid)}).
		File("/repo/bin/Release/" + string(rid) + "/mageesoft-pdx-ce-sav")
}

// Build the linux-x64 and linux-arm64 AOT binaries
func (m *ParadoxClausewitzSav) BuildAot(
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.Directory {
	return dag.Directory().
		WithFile("linux-x64/mageesoft-pdx-ce-sav", m.PublishAot(repoDir, LinuxX64)).
		WithFile("linux-arm64/mageesoft-pdx-ce-sav", m.PublishAot(repoDir, LinuxArm64))
}

// dotnet run the MageeSoft.PDX.CE.Tests project
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
		WithExec([]string{"dotnet", "run", "--project", "MageeSoft.PDX.CE.Tests"}).
		Stdout(ctx)
}

// dotnet test the MageeSoft.PDX.CE.Tests project
func (m *ParadoxClausewitzSav) Test(
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
		WithExec([]string{"dotnet", "test"}, dagger.ContainerWithExecOpts{
			Expect: dagger.ReturnTypeAny,
		})
}

// runs the CLI tool in a container and returns the output.
func (m *ParadoxClausewitzSav) CliTest(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) (string, error) {
	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithMountedFile("/root/.paradoxlauncher/Stellaris/save games/my test empire/ironman.sav", dag.CurrentModule().Source().File("saves/stellaris/ironman.sav")).
		WithWorkdir("/repo/src/MageeSoft.PDX.CE.Cli").
		WithExec([]string{"dotnet", "run", "--", "list"}).
		WithExec([]string{"dotnet", "run", "--", "query", "-n", "1", "-q", "player"}).
		Stdout(ctx)
}

// dotnet pack the MageeSoft.PDX.CE library and return the nupkg dir.
func (m *ParadoxClausewitzSav) Pack(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.Directory {
	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithWorkdir("/repo/src/MageeSoft.PDX.CE").
		WithExec([]string{"dotnet", "pack", "-c", "Release", "-o", "/repo/nupkgs"}).
		Directory("/repo/nupkgs")
}

// dotnet pack the MageeSoft.PDX.CE.Cli dotnet-tool and return the nupkg dir.
func (m *ParadoxClausewitzSav) PackTool(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.Directory {
	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithWorkdir("/repo/src/MageeSoft.PDX.CE.Cli").
		WithExec([]string{
			"dotnet",
			"pack",
			"-c",
			"Release",
			"/p:PackAsTool=true",
			"/p:PublishAot=false",
			"-o",
			"/repo/nupkgtool",
		}).
		Directory("/repo/nupkgtool")
}

// dotnet test the MageeSoft.PDX.CE.Tests project and return the coverage report.
func (m *ParadoxClausewitzSav) Coverage(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.File {
	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithWorkdir("/repo/src/MageeSoft.PDX.CE.Tests").
		WithExec([]string{
			"dotnet", "run", "--coverage", "--coverage-output", "/repo/report.cobertura.xml", "--coverage-output-format", "cobertura",
		}).
		File("/repo/report.cobertura.xml")
}
