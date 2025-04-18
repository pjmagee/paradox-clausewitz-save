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

func (m *ParadoxClausewitzSav) Agent(
	ctx context.Context,
// Assignment
	assignment string,

// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.Container {

	ws := dag.AgentWorkspace().Write("assignment.txt", assignment)

	env := dag.Env(dagger.EnvOpts{}).
		WithAgentWorkspaceInput("workspace", ws, "The workspace for the assignment").
		WithAgentWorkspaceOutput("workspace", "The workspace with results of the assignment")

	return dag.LLM().
		WithEnv(env).
		WithPrompt("You are an expert C# .NET Programmer and QA. You have access to a workspace.").
		WithPrompt("Complete the assignment found in the workspace, assignment.txt").
		WithPrompt("Do not wipe the state of the container. Focus on withExec, file, directory, and container commands.").
		Env().
		Output("results").
		AsContainer()
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
		WithWorkdir("/repo/src/MageeSoft.PDX.CE.Cli").
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
		WithWorkdir("/repo/src/MageeSoft.PDX.CE.Cli").
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
		WithWorkdir("/repo/src/MageeSoft.PDX.CE.Cli").
		WithExec([]string{"dotnet", "publish", "-c", "Release", "-r", string(rid), "-o", "/repo/bin/Release/" + string(rid)}).
		File("/repo/bin/Release/" + string(rid) + "/mageesoft-pdx-ce-sav")
}

func (m *ParadoxClausewitzSav) BuildNativeLinux(
// +defaultPath="./"
// +ignore=["**/obj", "**/bin"]
	repoDir *dagger.Directory,
) *dagger.Directory {
	return dag.Directory().
		WithFile("linux-x64/mageesoft-pdx-ce-sav", m.PublishAot(repoDir, LinuxX64)).
		WithFile("linux-arm64/mageesoft-pdx-ce-sav", m.PublishAot(repoDir, LinuxArm64))
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
		WithExec([]string{"dotnet", "run", "--project", "MageeSoft.PDX.CE.Tests"}).
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
	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
		WithMountedCache("/root/.nuget/packages", dag.CacheVolume("nuget")).
		WithMountedDirectory("/repo", repoDir).
		WithMountedFile("/root/.paradoxlauncher/Stellaris/save games/my test empire/ironman.sav", dag.CurrentModule().Source().File("saves/stellaris/ironman.sav")).
		WithWorkdir("/repo/src/MageeSoft.PDX.CE.Cli").
		WithExec([]string{"dotnet", "run", "--", "list"}).
		Stdout(ctx)
}
