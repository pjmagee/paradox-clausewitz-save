# Paradox Save File Structures and Parser Representation

This document outlines the distinct data structures commonly found in Paradox Interactive save files (like `.sav` gamestate files) and how the `MageeSoft.PDX.CE` parser represents them in C# using types like `SaveObject`, `SaveArray`, and `Scalar<T>`.

## 1. Scalar Key-Value Pair

* **Description:** The most basic structure. A key (string) maps directly to a single primitive value.
  * Handles standard types: `int`, `long`, `float`, `string`.
  * Booleans: `yes` maps to `true`, `no` maps to `false`.
  * Dates/Guids: Expected within quotes (`"yyyy.mm.dd"`, `"guid-string"`).
  * Null Strings: The literal value `none` is parsed as `null` for strings.
* **Example:**

    ```pdx
    name="Test Empire"
    year=2200
    rate=0.75
    is_active=yes
    last_updated="2250.10.20"
    capital_id="a1b2c3d4-e5f6-7890-1234-567890abcdef"
    leader_title=none
    ```

* **Parser Representation:** Represented as a `KeyValuePair<string, SaveElement>` within a parent `SaveObject`. The `SaveElement` will be a specific `Scalar<T>` instance (e.g., `Scalar<string>`, `Scalar<int>`, `Scalar<bool>`, `Scalar<DateTime>`, `Scalar<Guid>`). The `TryGet...` extension methods on `SaveObject` provide direct access.

## 2. Nested Object

* **Description:** A key maps to a block enclosed in `{ ... }` containing further key-value pairs or other nested structures. Represents hierarchical data.
* **Example:**

    ```pdx
    settings={
        difficulty="normal"
        ironman=yes
    }
    ```

* **Parser Representation:** Represented as a `KeyValuePair<string, SaveElement>` where the `SaveElement` is a `SaveObject`. You access the nested properties via this `SaveObject`.

## 3. Simple Array/List (Scalar Values)

* **Description:** A key maps to a block `{ ... }` containing a space-separated sequence of primitive values (can be mixed, but often uniform).
* **Example:**

    ```pdx
    member_ids={ 101 102 105 210 }
    resource_modifiers={ 1.0 0.5 1.25 }
    ```

* **Parser Representation:** Represented as a `KeyValuePair<string, SaveElement>` where the `SaveElement` is a `SaveArray`. The `SaveArray.Items` property contains a `List<SaveElement>`, where each element is typically a `Scalar<T>` corresponding to the values in the list.

## 4. Array/List of Objects

* **Description:** A key maps to a block `{ ... }` containing a sequence of *unnamed* nested object blocks `{ ... }`.
* **Example:**

    ```pdx
    fleets={ 
        { name="1st Fleet" ships=10 } 
        { name="Reserve Fleet" ships=5 } 
    }
    ```

* **Parser Representation:** Represented as a `KeyValuePair<string, SaveElement>` where the `SaveElement` is a `SaveArray`. The `SaveArray.Items` property contains a `List<SaveElement>`, where each element is a `SaveObject` representing one of the nested blocks.

## 5. Object as Dictionary (Integer Keys)

* **Description:** A key maps to a block `{ ... }` where the keys *inside* are integers, and values can be scalars or objects. Used for ID-based lookups.
* **Example:**

    ```pdx
    country_opinions={
        10={ value=50 relation="friendly" }
        15={ value=-25 relation="rival" }
    }
    ```

* **Parser Representation:** Represented as a `KeyValuePair<string, SaveElement>` where the `SaveElement` is a `SaveObject`. The `SaveObject.Properties` list contains `KeyValuePair<string, SaveElement>` entries where the `Key` is the *string representation* of the integer ID (e.g., "10", "15") and the `Value` is the corresponding `SaveElement` (scalar or `SaveObject`).

## 6. Object as Dictionary (String Keys)

* **Description:** A key maps to a block `{ ... }` where the keys *inside* are strings, typically mapping to scalar values. If values are complex/mixed objects, it's usually treated as a standard Nested Object (Structure 2).
* **Example:**

    ```pdx
    traits={
        trait_intelligent="yes"
        trait_thrifty="no"
    }
    ```

* **Parser Representation:** Represented as a `KeyValuePair<string, SaveElement>` where the `SaveElement` is a `SaveObject`. The `SaveObject.Properties` list contains `KeyValuePair<string, SaveElement>` entries where the `Key` is the string key and the `Value` is the corresponding scalar `SaveElement`.

## 7. "PDX Dictionary" (Array of Key-Value Objects)

* **Description:** A specific pattern where a key maps to an *array* (`{ ... }`), and each element in the array is an object (`{ ... }`) containing exactly `key=` and `value=` properties. This allows for duplicate logical keys.
* **Example:**

    ```pdx
    event_modifiers={
        { key="stability_boost" value=10 }
        { key="research_bonus" value=0.15 }
        { key="stability_boost" value=5 } # Duplicate 'key'
    }
    ```

* **Parser Representation:** Represented as a `KeyValuePair<string, SaveElement>` where the `SaveElement` is a `SaveArray`. The `SaveArray.Items` contains `SaveObject` elements, each expected to have properties named "key" and "value".

## 8. Array of ID-Object Pairs

* **Description:** A specific array structure where each element is an unnamed, two-element structure: the first is an ID (usually integer), and the second is the associated data object.

* **Example:** (From `intel_manager`)

    ```pdx
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

* **Parser Representation:** Represented as a `KeyValuePair<string, SaveElement>` where the `SaveElement` is a `SaveArray` (e.g., `intelArray`). Each element *within* `intelArray.Items` is **also** typically parsed as a `SaveArray` (e.g., `itemArray`).
  * `itemArray.Items[0]` would be the ID (e.g., `Scalar<int>`).
  * `itemArray.Items[1]` would be the data object (`SaveObject`).
    This requires specific handling during binding, as demonstrated in `ParserTests.Parse_Object_List_With_Ids`.

## Clausewitz Format

### Stellaris uses .sav to store the save documents

There are two files that make up a Stellaris save document:

* `meta` - This contains the metadata for the save
* `gamestate` - The main save document that contains the game state

For the source generator, these files were extracted manually and placed into the Models Project.

They've been included in the `<AdditionalFiles>` section of the `.csproj` file with a custom extension `.csf`

The `.csf` files are processed by the `ClausewitzCodeGenerator` which is a custom source generator that generates C# code from the `.csf` files.

The file extension name is a shortname for "Clausewitz Save Format" and is not a standard file extension.
