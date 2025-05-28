using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class WaterAirControlVariables : VariableClass
{
	[SerializeField, Range(0f, 100f)]
	public float maxSpeed = 10f;

	[SerializeField, Range(0f, 500f)]
	public float maxAirAcceleration = 10f;

	[SerializeField, Range(-100f, 0f)]
	public float customGravityStrength;

	[SerializeField, Range(-200f, 0f)]
	public float maximumDownVelocity;

	[ReadOnly]
	[AllowNesting]
	public float antiAirTimer;

	[ReadOnly]
	[AllowNesting]
	public bool falling;
}
