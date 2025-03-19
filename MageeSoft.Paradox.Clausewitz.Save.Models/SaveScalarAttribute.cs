using System;

namespace MageeSoft.Paradox.Clausewitz.Save.Models;

[AttributeUsage(AttributeTargets.Property)]
public class SaveScalarAttribute : Attribute
{
    public string Name { get; }

    public SaveScalarAttribute(string name)
    {
        Name = name;
    }
} 