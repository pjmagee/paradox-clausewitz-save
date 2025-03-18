
# Examples

example=
{
    index_based_items=
    {
        1=
        {
            field_1="value_1"
            field_2=yes
            field_3=no
        }
        2=
        {
            field_1="value_1"
            field_2=yes
            field_3=no
        }
        3=
        {
            field_1="value_1"
            field_2=yes
            field_3=no
            some_nested_array=
            {
                {
                    field_1="value_1"
                }

                {
                    field_1="value_1"
                }
            }
        }
    }
    array_of_save_objects=
    {
        // Array of objects
        {
            key="value"
        }

        {
            key="value"
        }
    }

    array_of_ints=
    {
        1
        2
        3
    }

    array_of_floats=
    {
        1.0
        2.0
        3.0
    }

    array_of_strings=
    {
        "string1"
        "string2"
        "string3"
    }    
}