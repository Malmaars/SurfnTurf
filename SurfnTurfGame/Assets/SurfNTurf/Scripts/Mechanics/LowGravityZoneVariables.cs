using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class LowGravityZoneVariables : VariableClass
{
    [ReadOnly]
    [AllowNesting]
    public bool isLowGravity = false;
    public float gravityModifier = 0.25f;
}

