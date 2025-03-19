# Stellaris Save Parser

A parser for Stellaris save files.

## Binding Architecture

The save parser uses a cascading binding architecture:

1. **Reflection-based binding** (currently used)
   - `ReflectionBinder` uses reflection to bind SaveObject instances to model classes
   - Fully supports cascading binding through hierarchical model structures
   - Automatically handles collections of complex objects
   - Supports arrays, nested objects, dictionaries
   - Performance is acceptable for small to medium sized saves, but could be improved

2. **Source-generated binding** (work in progress)
   - Source generator creates `Bind()` methods for classes marked with `[SaveModel]`
   - Cascading binding through the object graph
   - For complex objects, calls their `Bind()` method recursively
   - For collections, binds each element using appropriate binding
   - Will provide better performance than reflection-based binding

See [ROADMAP.md](ROADMAP.md) for a detailed plan of the source generator implementation.

## Roadmap for Source Generator Implementation

The following improvements are needed for the source generator to fully support cascading binding:

1. Dictionary/ImmutableDictionary Support:
   - Fix handling of Dictionary<TKey, TValue> with complex value types
   - Add support for ImmutableDictionary<TKey, TValue>
   - Add special handling for number keys (int, long)

2. Complex Object Binding:
   - Properly detect when a type has a Bind() method available
   - For SaveIndexedDictionary with complex values, invoke the Bind() method
   - Fix handling of arrays of complex objects

3. Required Properties Initialization:
   - Improve default value generation for required properties
   - Better default value generation for complex objects 

4. Avoid Duplicate Code Generation:
   - Fix the issue with duplicate initialization

5. Test Coverage:
   - Create comprehensive tests that compare reflection-based and source-generated binding
   - Verify identical results between both approaches
   - Benchmark performance differences

## Model Annotation

Models use attributes to mark properties for binding:

- `[SaveScalar("key")]` - Binds a scalar value (string, int, etc.)
- `[SaveArray("key")]` - Binds an array or list of values
- `[SaveObject("key")]` - Binds a nested object
- `[SaveIndexedDictionary("key")]` - Binds a dictionary where keys are in the property names

## Usage

```csharp
// Load a Stellaris save file
var save = StellarisSave.FromSave("path/to/save.sav");

// Access save data through the model
var galacticCommunity = save.GameState.Galaxy.GalacticCommunity;
var countries = save.GameState.Countries;
```

## Implementation Status

### Current Implementation
- Using `ReflectionBinder` for all model binding
- Tests for reflection-based binding are passing
- Model structure is well-defined and working
- Cascading binding works correctly through reflection

### Source Generator (Pending Implementation)
The source generator for model binding is partially implemented but has several issues that need to be fixed:

1. **Dictionary Handling**
   - Need to correctly support dictionaries with complex object values
   - ImmutableDictionary binding has issues in generated code

2. **Complex Object Arrays**
   - Need to ensure arrays of complex objects are correctly bound

3. **Cascading Binding**
   - Ensure nested objects correctly call their respective Bind methods
   - Fix syntax issues in the generated code for nested binding

4. **Testing**
   - Need to add comprehensive tests that verify source-generated binding
   - Add comparison tests that verify both binding approaches produce identical results

### Development Plan
1. Fix the source generator implementation (see BinderGenerator.cs comments)
2. Add comprehensive tests for source-generated binding
3. Update StellarisSave.cs to use source-generated binding
4. Compare performance between reflection and source-generated approaches
