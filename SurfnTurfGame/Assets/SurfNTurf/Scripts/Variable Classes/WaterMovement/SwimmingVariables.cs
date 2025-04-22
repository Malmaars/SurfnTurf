using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class SwimmingVariables : VariableClass
{
    [ReadOnly]
    [AllowNesting]
    public bool swimming;

	[SerializeField, Range(0f, 100f)]
	public float maxSpeed = 10f;

	[SerializeField, Range(0f, 500f)]
	public float maxAcceleration = 10f;
}
