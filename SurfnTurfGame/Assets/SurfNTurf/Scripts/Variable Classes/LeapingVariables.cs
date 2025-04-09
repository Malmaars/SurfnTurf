using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine;

[System.Serializable]
public class LeapingVariables : VariableClass
{
	public bool leapingResetsVelocity;
	public bool leapingResetsDash;
	[SerializeField, Range(0f, 100f)]
	public float upwardSpeed;
	[SerializeField, Range(0f, 100f)]
	public float forwardSpeed;

	[SerializeField, Range(0f, 10f)]
	public float maxDistanceFromGround;

	[SerializeField, Range(0f, 10f)]
	public float leapLength;

	[ReadOnly]
	[AllowNesting]
	public float leapLengthTimer;

	[SerializeField, Range(0f, 2f)]
	public float leapControlTime;

	[ReadOnly]
	[AllowNesting]
	public float leapControlTimer;

	[SerializeField, Range(0f, 2f)]
	public float leapCoyoteTime;

	[ReadOnly]
	[AllowNesting]
	public float leapCoyoteTimer;

	[ReadOnly]
	[AllowNesting]
	public bool desiredLeap;
	[ReadOnly]
	[AllowNesting]
	public bool leapAvailable;

	[ReadOnly]
	[AllowNesting]
	public bool leapAnimation;

	[ReadOnly]
	[AllowNesting]
	public bool leapt;

	[ReadOnly]
	[AllowNesting]
	public bool leaping;

	public UnityEvent onLeap;
}