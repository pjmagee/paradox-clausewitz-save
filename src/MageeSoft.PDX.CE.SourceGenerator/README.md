# MageeSoft.PDX.CE.SourceGenerator

This is a source generator for the MageeSoft.PDX.CE library.

## Usage

To use the source generator, add the following to your project file:

```xml
<ItemGroup>
    <ProjectReference Include="..\MageeSoft.PDX.CE.SourceGenerator\MageeSoft.PDX.CE.SourceGenerator.csproj" Private="true" />
</ItemGroup>
```

Documentation: <https://github.com/dotnet/roslyn/blob/main/docs/features/incremental-generators.cookbook.md>

The idea is to use the source generator to generate the PDX model classes from a CSF file.

The CSF file is a simple text file that contains the definitions that are based on data contained in the paradox clausewitz engine gamestate files.

The idea is for each game, we can generate the model classes for that game based on the CSF file.
