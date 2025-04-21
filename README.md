# Paradox Clausewitz Save CLI

A command-line tool for querying and modifying Paradox Clausewitz save files (e.g., Stellaris, EU4, HOI4, CK3, Vic3).

| Feature                          | Description                            | Example (based on real gamestate data) |
|----------------------------------|----------------------------------------|----------------------------------------|
| Path-based query                 | Query a specific value by full path    | `query -q "country.0.name"`            |
| Array expansion                  | Query all values in an array           | `query -q "fleet_template.[*].fleet_size"` |
| Recursive key search             | Find all values for a key              | `query -q ".. \| .energy?" --show-paths` |
| Recursive value search           | Find all values matching a string      | `query -q '.. \| select(. == "alloys")' --show-paths` |
| Recursive value substring search | Find all values containing a substring | `query -q '.. \| select(contains("5070.07.20"))' --show-paths` |
| Set a value by path              | Set a specific value at a path         | `set -q "country.0.resources.energy" -v 50000` |
| Set an array                     | Replace an array at a path             | `set -q "fleet_template.0.fleet_template_design" -v "{ { design=123 count=1 } { design=456 count=2 } }"` |
| Show paths in output             | Output both the value and its path     | `query -q ".. \| .energy?" --show-paths` |
| Query by save file or index      | Use -s or -n to select the save file   | `query -s ./ironman.sav -q "country.0.name"` or `query -n 1 -q "country.0.name"` |

## Realistic Examples

### Query all fleet sizes

```sh
query -q "fleet_template.[*].fleet_size"
```

### Find all values for a key anywhere (e.g., all 'energy' values)

```sh
query -q ".. | .energy?" --show-paths
```

### Find all values matching a string (e.g., all values equal to 'alloys')

```sh
query -q '.. | select(. == alloys)' --show-paths
```

### Find all values containing a substring (e.g., all dates containing '5070.07.20')

```sh
query -q '.. | select(contains("5070.07.20"))' --show-paths
```

### Set a specific value

```sh
set -q "country.0.resources.energy" -v 50000
```

### Replace an array

```sh
set -q "fleet_template.0.fleet_template_design" -v "{ { design=123 count=1 } { design=456 count=2 } }"
```

### Query by file path

```sh
query -s ./ironman.sav -q "country.0.name"
```

### Query by index (from list command)

```sh
query -n 1 -q "country.0.name"
```

---

For more, see the help output (`query --help`, `set --help`).

## PDX Clausewitz Save File Structures
 
This document outlines the distinct data structures commonly found in PDX Interactive save
 
 ## Scalars
 
 * **Description:** The most basic structure. A key (string) maps directly to a single primitive value.
   * types: `int32`, `int64`, `float`, `bool`, `string`, `date`, `guid`
 
   * **Example:**

       ```pdx
       int32=2200
       int64=123456789012345 
       float=3.14159
       bool=yes
       string_unquoted=hello
       string_quoted="Hello, World!"
       date="2200.01.01"    
       capital_id="a1b2c3d4-e5f6-7890-1234-567890abcdef"       
       ```
 
 ## Blocks
 
 * **Description:** A key maps to a block enclosed in `{ ... }` containing further key-value pairs or other nested structures. Represents hierarchical data.
 * **Example:**
 
     ```pdx
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
 
     ```pdx
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
 
     ```pdx
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
 
     ```pdx
     intel={
         { 
             67108916 
             { intel=10 stale_intel={ } } 
         }
         { 
             218103860 
             { intel=10 stale_intel={ } } 
         }
     }
     ```
 
 ## Clausewitz Save Archive
 
 ### Stellaris
 
 * `meta` - This contains the metadata for the save
 * `gamestate` - The main save document that contains the game state
