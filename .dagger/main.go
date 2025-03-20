package main

import (
	"context"
	"dagger/paradox-clausewitz-sav/internal/dagger"
)

type ParadoxClausewitzSav struct{}

// Build the project
func (m *ParadoxClausewitzSav) Build(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	directoryArg *dagger.Directory,
) *dagger.Container {

	cacheVolume := dag.CacheVolume("nuget")

	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:9.0").
		WithMountedCache("/root/.nuget/packages", cacheVolume).
		WithMountedDirectory("/mnt", directoryArg).
		WithWorkdir("/mnt").
		WithExec([]string{"dotnet", "build"})
}

func (m *ParadoxClausewitzSav) VsTest(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	directoryArg *dagger.Directory,
) (string, error) {

	cacheVolume := dag.CacheVolume("nuget")

	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:9.0").
		WithMountedCache("/root/.nuget/packages", cacheVolume).
		WithMountedDirectory("/mnt", directoryArg).
		WithWorkdir("/mnt").
		WithExec([]string{"dotnet", "run", "--project", "MageeSoft.Paradox.Clausewitz.Save.Tests"}).
		Stdout(ctx)
}

func (m *ParadoxClausewitzSav) Test(
	ctx context.Context,
	// +defaultPath="./"
	// +ignore=["**/obj", "**/bin"]
	directoryArg *dagger.Directory,
) (string, error) {

	cacheVolume := dag.CacheVolume("nuget")

	return dag.Container().
		From("mcr.microsoft.com/dotnet/sdk:9.0").
		WithMountedCache("/root/.nuget/packages", cacheVolume).
		WithMountedDirectory("/mnt", directoryArg).
		WithWorkdir("/mnt").
		WithExec([]string{"dotnet", "test"}).
		Stdout(ctx)
}
