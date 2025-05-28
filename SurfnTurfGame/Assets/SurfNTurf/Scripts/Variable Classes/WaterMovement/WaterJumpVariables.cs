using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class WaterJumpVariables : VariableClass
{
	[ReadOnly]
	[AllowNesting]
	public bool desiredJump;

	[SerializeField, Range(0f, 50f)]
	public float JumpForce;
}
