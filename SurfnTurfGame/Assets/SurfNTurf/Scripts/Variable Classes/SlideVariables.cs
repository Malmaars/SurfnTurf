using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class SlideVariables : VariableClass
{
	[SerializeField, Range(0f, 50f)]
	public float boostPower;

	[SerializeField, Range(0f, 10f)]
	public float slideDuration;

	[ReadOnly]
	[AllowNesting]
	public float slideDurationTimer;

	[ReadOnly]
	[AllowNesting]
	public bool sliding;

	[ReadOnly]
	[AllowNesting]
	public bool slid;

}
