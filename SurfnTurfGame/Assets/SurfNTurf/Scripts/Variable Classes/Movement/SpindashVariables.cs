using UnityEngine;
using NaughtyAttributes;

[System.Serializable]
public class SpindashVariables : VariableClass
{
	public bool hitResetsDuration;
	[ReadOnly]
	[AllowNesting]
	public bool spinDashing;
	[ReadOnly]
	[AllowNesting]
	public Vector3 spindDashDirection;

	[SerializeField, Range(0f, 100f)]
	public float speed;

	[SerializeField, Range(0f, 10f)]
	public float pushPower;

	[SerializeField, Range(0f, 10f)]
	public float duration;

	[ReadOnly]
	[AllowNesting]
	public float durationTimer;
	[SerializeField, Range(0f, 10f)]
	public float bounceCooldown;

	[ReadOnly]
	[AllowNesting]
	public float bounceCooldownTimer;

	[SerializeField, Range(0f, 2f)]
	public float spinDashCoyoteTime;

	[ReadOnly]
	[AllowNesting]
	public bool spindDashAnimation;

	[ReadOnly]
	[AllowNesting]
	public float spinDashCoyoteTimer;
	
	[ReadOnly]
	public bool turnOffSpinDash;
}
