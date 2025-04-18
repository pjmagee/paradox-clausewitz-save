package main

import (
	"context"
	"dagger/agent-workspace/internal/dagger"
)

type AgentWorkspace struct {
	Container *dagger.Container
}

func New() AgentWorkspace {
	return AgentWorkspace{
		Container: dag.Container().
			From("mcr.microsoft.com/dotnet/sdk:10.0-preview").
			WithWorkdir("/src"),
	}
}

func (w *AgentWorkspace) Read(ctx context.Context, path string) (string, error) {
	return w.Container.File(path).Contents(ctx)
}

func (w AgentWorkspace) Write(path, content string) AgentWorkspace {
	w.Container = w.Container.WithNewFile(path, content)
	return w
}

func (w *AgentWorkspace) Build(ctx context.Context) error {
	_, err := w.Container.WithExec([]string{"dotnet", "build", "./..."}).Stderr(ctx)
	return err
}
