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
