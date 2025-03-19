# Source Generator Implementation Plan

## Current Issues

Based on the build errors, we need to address several issues with the source generator:

1. **Position and other structs handling**: Structs like `Position` don't have a `Bind()` method and need special initialization
2. **Complex object binding**: Some required complex objects aren't being properly initialized
3. **Dictionary key conversion**: Issues with dictionary key parsing, especially for numeric keys
4. **Type detection**: Incorrect detection of whether a type has a Bind method
5. **Lambda expression errors**: Invalid conversions in lambda expressions for dictionary key/value handling

## Specific Issues Found During Testing

During our testing, we discovered these specific issues:

1. **Position Struct Issues**:
   - Variable naming conflicts in the Position struct initialization
   - Duplicate variables (x, y, z) causing errors (CS0128: A local variable named 'x' is already defined)

2. **Required Properties Not Initialized**:
   - Issues with `CS9035: Required member must be set in the object initializer` errors
   - Examples: Position.X, Position.Y, Position.Z
   - Examples: Planet.Size, Planet.Position, etc.
   - Examples: Pop.Faction, Pop.Id, Pop.Happiness, Pop.Power

3. **Dictionary Binding Issues**:
   - Cannot convert long[] to Dictionary<long, Planet> (CS0029)
   - Issues with Dictionary<TKey, TValue> expressions

4. **Special Types Without Bind Methods**:
   - FleetMovementManager (CS0117: does not contain a definition for 'Bind')
   - Megastructure (CS0117: does not contain a definition for 'Bind')
   - Combat.Coordinate not set properly

5. **Lambda Expression Type Errors**:
   - Cannot convert lambda expression (CS1662)
   - Cannot implicitly convert type 'int' to 'bool' (CS0029)

## Implementation Plan

### 1. Fix Position and other struct binding

These structs need special handling since they don't have a `Bind()` method:

```csharp
// For Position struct
if (propertyType.Name == "Position")
{
    sb.AppendLine($"                    {propertyName} = obj.TryGetSaveObject(\"{propertyKey}\", out var {varName}Obj) ?");
    sb.AppendLine($"                        new Position");
    sb.AppendLine($"                        {{");
    sb.AppendLine($"                            X = {varName}Obj.TryGetFloat(\"x\", out var posX) ? posX : 0,");
    sb.AppendLine($"                            Y = {varName}Obj.TryGetFloat(\"y\", out var posY) ? posY : 0,");
    sb.AppendLine($"                            Z = {varName}Obj.TryGetFloat(\"z\", out var posZ) ? posZ : 0");
    sb.AppendLine($"                        }} :");
    sb.AppendLine($"                        default,");
    return;
}
```

### 2. Fix Required Property Handling

For required properties, ensure they're always initialized:

```csharp
// Check if this is a required property
bool isRequired = property.IsRequired;
if (isRequired)
{
    // Make sure to initialize with default values
    if (propertyType.IsReferenceType)
    {
        sb.AppendLine($"                    {propertyName} = {GetDefaultValueWithNew(propertyType)},");
    }
}
```

### 3. Fix Dictionary Binding

Update dictionary binding to handle numeric keys correctly:

```csharp
private static string GetKeyParsingCode(string keyTypeName)
{
    if (keyTypeName == "System.String" || keyTypeName == "string")
    {
        return "kvp.Key";
    }
    else if (keyTypeName == "System.Int32" || keyTypeName == "int")
    {
        return "int.TryParse(kvp.Key, out var intKey) ? intKey : default(int)";
    }
    else if (keyTypeName == "System.Int64" || keyTypeName == "long")
    {
        return "long.TryParse(kvp.Key, out var longKey) ? longKey : default(long)";
    }
    else
    {
        return $"default({keyTypeName})";
    }
}
```

### 4. Improve Type Detection

Update the IsComplexType method to correctly detect types that need Bind():

```csharp
private static bool IsComplexType(ITypeSymbol type)
{
    if (type == null)
        return false;

    // Check if it's a simple type
    if (SimpleTypes.Contains(type.ToDisplayString()))
        return false;

    // Check if it's a struct
    if (type.TypeKind == TypeKind.Struct)
        return false;

    // Check if it's an enum
    if (type.TypeKind == TypeKind.Enum)
        return false;

    // Check if it's a collection
    if (type.ToDisplayString().Contains("Collection") ||
        type.ToDisplayString().Contains("List") ||
        type.ToDisplayString().Contains("Dictionary") ||
        type.ToDisplayString().Contains("Array"))
        return false;

    return true;
}
```

### 5. Fix Lambda Expressions for Dictionary Conversions

Fix the lambda expressions in dictionary conversion:

```csharp
// For dictionary where key is int
sb.AppendLine($"                    {propertyName} = obj.TryGetSaveObject(\"{propertyKey}\", out var {varName}Dict) ?");
sb.AppendLine($"                        {varName}Dict.Properties");
sb.AppendLine($"                        .Where(kvp => int.TryParse(kvp.Key, out _))");
sb.AppendLine($"                        .ToDictionary(");
sb.AppendLine($"                            kvp => int.Parse(kvp.Key),");
sb.AppendLine($"                            kvp => {GetValueBindingCode(valueType)})");
sb.AppendLine($"                        : new Dictionary<int, {valueType}>(),");
```

### 6. Special Cases for FleetMovementManager and Other Complex Types

For special complex types like FleetMovementManager:

```csharp
// Check for special handling cases
if (propertyType.Name == "FleetMovementManager")
{
    sb.AppendLine($"                    {propertyName} = obj.TryGetSaveObject(\"{propertyKey}\", out var {varName}Obj) ?");
    sb.AppendLine($"                        new FleetMovementManager");
    sb.AppendLine($"                        {{");
    sb.AppendLine($"                            MovementTarget = {varName}Obj.TryGetInt(\"movement_target\", out var target) ? target : 0,");
    sb.AppendLine($"                            State = {varName}Obj.TryGetString(\"state\", out var state) ? state : string.Empty,");
    // Add all required properties
    sb.AppendLine($"                        }} :");
    sb.AppendLine($"                        new FleetMovementManager(),");
    return;
}
```

### 7. Modify the Object Creation Approach

Instead of trying to use cascading binding for all types, adopt a hybrid approach:

```csharp
private static void GenerateObjectBinding(StringBuilder sb, string propertyName, string propertyKey, string varName, ITypeSymbol propertyType)
{
    // Check if this type has a SaveModel attribute (and therefore has a Bind method)
    bool hasSaveModelAttribute = propertyType.GetAttributes()
        .Any(a => a.AttributeClass?.Name == "SaveModelAttribute" || a.AttributeClass?.Name == "SaveModel");

    if (hasSaveModelAttribute)
    {
        // Use cascading binding with Bind method
        sb.AppendLine($"                    {propertyName} = obj.TryGetSaveObject(\"{propertyKey}\", out var {varName}Obj) ?");
        sb.AppendLine($"                        {propertyType.ToDisplayString()}.Bind({varName}Obj) :");
        sb.AppendLine($"                        new {propertyType.ToDisplayString()}(),");
    }
    else
    {
        // Use direct property-by-property binding for types without Bind method
        // Add special handling for known complex types
        GenerateSpecialTypeBinding(sb, propertyName, propertyKey, varName, propertyType);
    }
}
```

## Incremental Implementation Approach

1. **Focus on simple models first**:
   - Enable the source generator only for the TestModelForSourceGen and PositionTestModel
   - Get these passing before proceeding to more complex models

2. **Add special case handlers**:
   - Create a registry of special handlers for more complex types
   - Implement handlers for Position, FleetMovementManager, etc.

3. **Fix the dictionary logic**:
   - Focus on properly handling numeric keys in dictionaries
   - Ensure proper conversion between regular and immutable dictionaries

4. **Add support for cascading binding**:
   - Fix detection of which types support Bind()
   - Implement the cascade for complex object hierarchies

## Testing Plan

1. Start with simple models and gradually work toward more complex ones:
   - Test with simple scalar properties
   - Test with array/list properties
   - Test with complex nested objects
   - Test with dictionaries (both string and numeric keys)

2. Create specific test cases for known problematic patterns:
   - Dictionary<long, ComplexObject>
   - Array/List of ComplexObject
   - Structs like Position
   - Required properties with complex types

3. Implement comparison tests to verify that source-generated binding produces identical results to reflection-based binding. 