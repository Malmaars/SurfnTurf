using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class DiveVariables : VariableClass
{
	public bool divingResetsVelocity;

	[SerializeField, Range(0f, 100f)]
	public float upwardSpeed;
	[SerializeField, Range(0f, 100f)]
	public float forwardSpeed;

	[SerializeField, Range(0f, 1f)]
	public float pushPower;

	[ReadOnly]
	[AllowNesting]
	public Vector3 divingDirection;

	[SerializeField, Range(0f, 10f)]
	public float diveLength;

	[ReadOnly]
	[AllowNesting]
	public float diveLengthTimer;

	[ReadOnly]
	[AllowNesting]
	public bool desiredDive;
	[ReadOnly]
	[AllowNesting]
	public bool diveAvailable;

	[ReadOnly]
	[AllowNesting]
	public bool dived;

	[ReadOnly]
	[AllowNesting]
	public bool diving;

	public UnityEvent onDive;
}
