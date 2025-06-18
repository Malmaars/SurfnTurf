using System;

[AttributeUsage(AttributeTargets.Method)]
public class CommandAttribute : Attribute
{
    public string Name;

    public CommandAttribute(string name)
    {
        Name = name;
    }
}
