using System;

namespace MageeSoft.Paradox.Clausewitz.Save.Models;

[AttributeUsage(AttributeTargets.Property)]
public class SaveObjectAttribute : Attribute
{
    public string Name { get; }

    public SaveObjectAttribute(string name)
    {
        Name = name;
    }
} 