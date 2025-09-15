
using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class CanonVariables : VariableClass
{
    [ReadOnly]
    [AllowNesting]
    public bool isLaunching = false;

}
