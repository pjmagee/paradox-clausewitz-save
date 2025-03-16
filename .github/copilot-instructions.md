# FACTS

- The ironman.sav contains all data files already that are also in testdata folder (zipped)
- testdata files - parser handles yes/no into Scalar<bool>
- testdata files - parser handles valid date strings e.g date="01.12.2024" into Scalar<DateOnly>
- testdata files - parser handles number values WITH decimal (.) into Scalar<float>
- testdata files - parser handles number values WITHOUT decimal (.) into Scalar<int>

- SaveObject root contains 'properties' at the 'parent level' in the gamestate-category' 

e.g gamestate-achievement content:

```sample
achievement=
{
    achievement=
    {

    }
}
```

DONT:

- do not ignore comments that tell you to not change something
- do not edit sample data in the Stellaris/TestData folder
- do not change any .csproj sdk or test framework
- do notadd nuget packages
- do not include 'test logic' in the main code base
- do not design 'default' instances of Models/Classes/Records
- do not change the test framework

DO:

- add SaveNameAttribute on each field that is from gamestate files
- follow comments that tell you to NOT change files or code.
- understand test data (gamestate files) with object items with number keys can be used as a Property Id on a Model record
- focus on a single test class to fix at a time
- use the dotnet test --filter <Expression> when testing and fixing specific classes
- use the Tests_net9.0_x64.log for test output for debugging
- follow solid coding principles, Single Responsibility, Dependency Inversion, Interface Segregation etc
- review test data file structures for loading the data into the Model classes
- ensure Models use a Load function 
- ensure you use the SaveObject, SaveArray and Scalar<T>
- only test one 'item' in an array of models, e.g the first planet, the first pop, the first contact, the first buildings
- review your changes and ensure rules are followed
- ensure 'Load' functions are consistent across all Models classes
- ensure unit test assertions are correct against Model Property Types
- use dotnet build
- use dotnet test --filter <Expression>
