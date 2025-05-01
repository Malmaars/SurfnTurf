using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class WaterSwipeVariables : VariableClass
{
	[ReadOnly]
	[AllowNesting]
	public bool desiredSwipe;
}
