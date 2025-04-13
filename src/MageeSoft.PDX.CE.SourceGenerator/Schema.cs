namespace MageeSoft.PDX.CE.SourceGenerator
{
    // This file contains most of the data structures needed for schema inference and code generation
    // These types are used by IncrementalGenerator.cs but are separated for better organization
    
    /// <summary>
    /// Represents a schema inferred from a SaveObject in a .sav file
    /// </summary>
    public class Schema
    {
        public string Name { get; set; } = "";
        public List<SchemaProperty> Properties { get; set; } = new List<SchemaProperty>();
        
        // For specialized collection types
        public bool IsSpecializedCollection { get; set; } = false;
        public string CollectionElementType { get; set; } = "string";
        public string CollectionSourceKey { get; set; } = "";
        public bool IsArrayCollection { get; set; } = false; // True for collections that contain arrays like asteroid_postfix={ "values" }
    }

    /// <summary>
    /// Represents a property in a schema inferred from a SaveObject
    /// </summary>
    public class SchemaProperty
    {
        public string KeyName { get; set; } = "";
        public string PropertyName { get; set; } = "";
        public string Type { get; set; } = "object";
        public bool IsNullable { get; set; } = true;
        public SaveDataType SaveDataType { get; set; } = SaveDataType.Unknown;
        public SaveDataType ElementSaveDataType { get; set; } = SaveDataType.Unknown; // For arrays/lists
        public SaveDataType KeySaveDataType { get; set; } = SaveDataType.Unknown; // For dictionaries with non-string keys
        public bool HasDictionaryNullValues { get; set; } = false; // Specifically for dictionaries, indicates if value type should be T?
        public Schema? NestedSchema { get; set; } // For objects or elements/values that are objects
        public Schema? SpecializedCollectionSchema { get; set; } // For specialized collection types like AsteroidPostfixCollection
        public string RepeatedKey { get; set; } = ""; // For serialization of repeated key patterns like trait="value1" trait="value2"
        
        // New properties for attribute-based serialization
        public bool HasCustomAttributes { get; set; } = false; // Whether to add custom attributes 
        public bool IsArrayValue { get; set; } = false; // Whether this is an array value for repeated keys
        
        // Track observed types for numeric properties to enable better type promotion
        public HashSet<SaveDataType> ObservedNumericTypes { get; } = new HashSet<SaveDataType>();
        
        // Add a type to the observations and promote the property type if needed
        public void AddObservedNumericType(SaveDataType type)
        {
            if (type != SaveDataType.Int && type != SaveDataType.Long && type != SaveDataType.Float)
                return;
                    
            ObservedNumericTypes.Add(type);
            
            // Apply type promotion if needed
            if (type == SaveDataType.Float || this.SaveDataType == SaveDataType.Float)
            {
                // Float is highest priority
                this.SaveDataType = SaveDataType.Float;
                Type = "float";
            }
            else if ((type == SaveDataType.Long && this.SaveDataType == SaveDataType.Int) || 
                     (type == SaveDataType.Int && this.SaveDataType == SaveDataType.Long) ||
                     this.SaveDataType == SaveDataType.Long)
            {
                // Long is second priority
                this.SaveDataType = SaveDataType.Long;
                Type = "long";
            }
            else if (this.SaveDataType == SaveDataType.Unknown || this.SaveDataType == SaveDataType.String)
            {
                // First numeric type encountered
                this.SaveDataType = type;
                Type = GetTypeName(type);
            }
        }
        
        private string GetTypeName(SaveDataType type) => type switch
        {
            SaveDataType.Int => "int",
            SaveDataType.Long => "long",
            SaveDataType.Float => "float",
            _ => "object"
        };
    }

    /// <summary>
    /// Represents the inferred data type of a property based on its representation 
    /// in the Paradox save file format.
    /// </summary>
    public enum SaveDataType
    {
        /// <summary>Type could not be determined or is inconsistent.</summary>
        Unknown,
        /// <summary>Represents quoted strings ("value") or unquoted identifiers (token).</summary>
        String,
        /// <summary>Represents integer numbers (e.g., 123).</summary>
        Int,
        /// <summary>Represents large integer numbers (e.g., 9999999999).</summary>
        Long,
        /// <summary>Represents floating-point numbers (e.g., 1.23, -0.5).</summary>
        Float,
        /// <summary>Represents boolean values (yes/no).</summary>
        Bool,
        /// <summary>Represents dates (e.g., "1066.10.14").</summary>
        DateTime, 
        /// <summary>Represents GUID values (e.g., "guid").</summary>
        Guid,
        /// <summary>Represents a nested object structure (key = { ... }).</summary>
        Object,
        /// <summary>Represents an array/list structure (key = { value1 value2 ... }).</summary>
        Array,
        /// <summary>Represents a dictionary-like object where keys are integers (key = { 1 = ... 2 = ... }).</summary>
        /// <summary>Represents a dictionary-like object where keys are numeric (int or long) (key = { 123 = ... 987 = ... }).</summary>
        DictionaryNumericKey, // Potentially merge with IntKey if longs aren't needed as keys
        /// <summary>Represents a standard dictionary-like object with string keys (key = { subkey1 = ... subkey2 = ... }).</summary>
        DictionaryStringKey,
        /// <summary>Represents a list of pairs, often used for ID->Object mapping (key = { { id1 { ... } } { id2 { ... } } ... }).</summary>
        DictionaryScalarKeyObjectValue, // e.g. { { key1=val1 } { key2=val2 } ... }
        /// <summary>Represents repeated keys mapped to a list of strings (key = val1 key = val2 ...).</summary>
        RepeatedKeyStringList, // e.g., flag = x flag = y -> List<string>
        /// <summary>Similar to RepeatedKeyStringList, often used for names or identifiers.</summary>
        FlatRepeatedKeyList, // e.g., name = "a" name = "b" -> List<string>
        /// <summary>Represents repeated keys mapped to a list of objects (key = { ... } key = { ... } ...).</summary>
        FlatRepeatedObjectList, // e.g., pop = {..} pop = {..} -> List<Pop>
        /// <summary>Represents repeated keys mapped to a list of arrays/lists (key = { a b } key = { c d } ...).</summary>
        RepeatedArrayList,
        /// <summary>Represents a dictionary with integer keys.</summary>
        DictionaryIntKey,
        /// <summary>Represents a dictionary with long keys.</summary>
        DictionaryLongKey
    }
} 