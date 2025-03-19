# Roadmap for Source Generator Implementation

This document outlines the detailed plan for implementing the source generator for automatic binding of save file data.

## Current State

- **Reflection-Based Binding**: Working correctly with full cascading binding support
- **Source Generator**: Basic implementation exists but has several issues with cascading binding

## Key Issues to Fix

1. **Dictionary Binding**:
   - Support `ImmutableDictionary<TKey, TValue>` in addition to regular `Dictionary<TKey, TValue>`
   - Fix type conversion for numeric keys (int/long parsing from strings)
   - Properly handle complex object values in dictionaries by cascading binding

2. **Complex Object Binding**:
   - Fix the detection of types that have a Bind() method
   - Ensure proper cascading through the object graph by calling Bind() on complex objects
   - Properly handle arrays of complex objects

3. **Required Properties Initialization**:
   - Improve default value generation for required properties
   - Fix initialization of complex object properties
   - Handle nullable reference types correctly

4. **Code Generation Improvements**:
   - Fix duplicate property initialization
   - Handle various collection types consistently (arrays, lists, dictionaries)
   - Generate properly formatted code that compiles correctly

## Implementation Strategy

1. **Phase 1: Fix Type Detection**
   - Improve detection of complex types vs primitive types
   - Add proper checks for types that have Bind() methods
   - Update type conversion logic for different numeric types

2. **Phase 2: Fix Dictionary Support**
   - Add support for ImmutableDictionary
   - Fix key parsing for numeric dictionary keys
   - Implement cascading binding for dictionary values

3. **Phase 3: Fix Collection Support**
   - Improve handling of arrays of complex objects
   - Ensure proper cascading binding for collection elements
   - Support for various collection types (arrays, lists, immutable collections)

4. **Phase 4: Property Initialization**  
   - Fix default value generation
   - Handle required properties properly
   - Improve initialization of complex objects

5. **Phase 5: Testing**
   - Expand test coverage for the source generator
   - Create tests that compare reflection-based and source-generated binding
   - Benchmark performance differences

## Test Plan

1. **Unit Tests**:
   - Test each binding type (scalar, array, object, dictionary)
   - Test cascading binding through multiple levels of nesting
   - Test with various collection types

2. **Integration Tests**:
   - Test with real Stellaris save files
   - Verify complete object graphs are bound correctly
   - Compare results with reflection-based binding

3. **Comparison Tests**:
   - Create tests that bind the same object with both methods
   - Verify identical results between reflection-based and source-generated binding
   - Measure and document performance differences 