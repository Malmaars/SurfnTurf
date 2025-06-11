using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class WaterDashVariables : VariableClass
{
	[ReadOnly]
	[AllowNesting]
	public bool desiredDash;
}
