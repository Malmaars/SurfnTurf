using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class AirDashVariables : VariableClass
{
	[ReadOnly]
	[AllowNesting]
	public bool airDashing;
}
