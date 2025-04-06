namespace MageeSoft.PDX.CE;

public enum SaveType
{
    /// <summary>
    /// A key=value of an object
    /// </summary>
    /// <example>
    /// key={ }
    /// key={ key1=value1 key2=value2 }
    /// key={ key1={} key2={ key2Key1=1 } key3={ key3Key1=1 key3Key2=2 } }
    /// </example>
    Object,

    /// <summary>
    /// A key=value of an array of items
    /// </summary>
    /// <example>
    /// Items could be any of the other types (Object, Scalar, Array)
    /// key={ item1 item2 item3 }
    /// key={ { } { } { } }
    /// key={ 1={} 2={} 3={} }
    /// key={ 1 2 3 } 
    /// </example>
    Array,

    /// <summary>
    /// Quoted value of string (explicitly treated as string)
    /// Quoted can allow for special characters, spaces, etc.
    /// </summary>
    /// <example>
    /// key="quoted"
    /// key="quoted string with spaces"
    /// key="quoted special characters: !@#$%^&*()_+"
    /// </example>
    String,

    /// <summary>
    /// Unquoted value (treated as string)
    /// </summary>
    /// <example>
    /// key=unquoted
    /// key=unquoted_string_with_spaces
    /// </example>
    Identifier,

    /// <summary>
    /// Unquoted value of 'yes' (true) or 'no' (false) 
    /// </summary>
    /// <example>
    /// key=no
    /// key=yes
    /// </example>
    Bool,

    /// <summary>
    /// Unquoted value of floating point number
    /// </summary>
    /// <example>
    /// key=1.0
    /// key=1.5
    /// </example>
    Float,

    /// <summary>
    ///  Unquoted value of integer number
    /// </summary>
    /// <example>
    /// key=239243902
    /// key=12392
    /// </example>
    Int32,

    /// <summary>
    /// Unquoted value of a 64bit integer number
    /// </summary>
    /// <example>
    /// key=9223372036854775807
    /// </example>
    Int64,

    /// <summary>
    /// Usually quoted, but parsed as a DateTime
    /// </summary>
    /// <example>
    /// key="2021.01.01" 
    /// </example>
    Date,

    /// <summary>
    /// Usually found quoted, but parsed as a Guid
    /// </summary>
    /// <example>
    /// key="00000000-0000-0000-0000-000000000000"
    /// </example>
    Guid
}