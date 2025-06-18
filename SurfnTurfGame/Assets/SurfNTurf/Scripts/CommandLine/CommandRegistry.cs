using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

public static class CommandRegistry
{
    private static Dictionary<string, MethodInfo> commandMap = new();

    // Call this once at start
    public static void RegisterAllCommands()
    {
        commandMap.Clear();
        foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (Type type in assembly.GetTypes())
            {
                foreach (MethodInfo method in type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static))
                {
                    var attr = method.GetCustomAttribute<CommandAttribute>();
                    if (attr != null)
                    {
                        commandMap[attr.Name] = method;
                    }
                }
            }
        }
    }

    public static void Execute(string commandName)
    {
        if (commandMap.TryGetValue(commandName, out var method))
        {
            method.Invoke(null, null); // assumes parameterless static methods
        }
        else
        {
            UnityEngine.Debug.Log($"Unknown command: {commandName}");
        }
    }
}
