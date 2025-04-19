# PDX Clausewitz Save File Structures and Parser Representation

This document outlines the distinct data structures commonly found in PDX Interactive save

## Scalars

* **Description:** The most basic structure. A key (string) maps directly to a single primitive value.
  * types: `int32`, `int64`, `float`, `bool`, `string`, `date`, `uuid`

* **Example:**

    ```toml
    int32=2200
    int64=123456789012345 
    float=3.14159
    bool=yes
    string="Hello, World!"
    date="2200.01.01"    
    capital_id="a1b2c3d4-e5f6-7890-1234-567890abcdef"
    leader_title=none
    ```

## Blocks

* **Description:** A key maps to a block enclosed in `{ ... }` containing further key-value pairs or other nested structures. Represents hierarchical data.
* **Example:**

    ```toml
    settings={
        difficulty="normal"
        ironman=yes
        child_settings={
            foo=1
            bar=2
            another_level={
                value=100
            }
        }
    }
    ```

## Arrays

* **Description:** A key maps to a block `{ ... }` containing a space-separated sequence of values. Values can be scalars, objects, or arrays.
* **Example:**

    ```toml
    integer_array={ 1 2 3 4 5 }
    float_array={ 1.1 2.2 3.3 4.4 5.5 }
    object_array={
        { key="stability_boost" value=10 }
        { key="research_bonus" value=0.15 }
        { key="stability_boost" value=5 }
    }
    array_of_arrays={
        { 1 2 3 }
        { 4 5 6 }
        { 7 8 9 }
    }
    ```

## Integer-keyed dictionaries

* **Description:** A key maps to a block `{ ... }` where the keys *inside* are integers, and values can be scalars or objects. Used for ID-based lookups.
* **Example:**

    ```toml
    integer_keyed_objects={
        1={ name="First Item" value=100 }
        2={ name="Second Item" value=200 }
        3={ name="Third Item" value=300 }
    }
    integer_keyed_scalars={
        1=100
        2=200
        3=300
    }
    ```

## ID-Object Pair arrays

* **Description:** A specific array structure where each element is an unnamed, two-element structure: the first is an ID (usually integer), and the second is the associated data object.

* **Example:** (From `intel_manager`)

    ```toml
    intel={
        { 
            67108916 
            { intel=10 stale_intel={} } 
        }
        { 
            218103860 
            { intel=10 stale_intel={} } 
        }
    }
    ```

## Clausewitz Save Archive

### Stellaris

* `meta` - This contains the metadata for the save
* `gamestate` - The main save document that contains the game state
