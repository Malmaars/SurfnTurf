using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class WaterJumpVariables : VariableClass
{
	[ReadOnly]
	[AllowNesting]
	public bool desiredJump;
}
