using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine;

[System.Serializable]
public class DashingVariables : VariableClass
{
	public bool threeDimensionalDash;
	public bool immediateStop;

	public bool fullDashControl;
	public bool alwaysDashToInput;
	public bool dashingResetsLeap;
	public bool dashingGivesExtraJump;
	public bool breakDashWithJump;

	[ReadOnly]
	[AllowNesting]
	public bool gravityOff;

	[SerializeField, Range(0f, 100f)]
	public float dashSpeed;

	[SerializeField, Range(0f, 10f)]
	public float dashCooldown;

	[ReadOnly]
	[AllowNesting]
	public float dashTimer;

	[ReadOnly]
	[AllowNesting]
	public Vector3 LastHorizontalDirection;

	[SerializeField, Range(0f, 10f)]
	public float dashLength;

	[ReadOnly]
	[AllowNesting]
	public float dashLengthTimer;

	[SerializeField, Range(0f, 2f)]
	public float dashControlTime;

	[ReadOnly]
	[AllowNesting]
	public float dashControlTimer;

	[SerializeField, Range(0f, 2f)]
	public float dashCoyoteTime = 0.2f;

	[ReadOnly]
	[AllowNesting]
	public float dashCoyoteTimer;

	[ReadOnly]
	[AllowNesting]
	public bool startDash;


	[ReadOnly]
	[AllowNesting]
	public bool desiredDash;

	[ReadOnly]
	[AllowNesting]
	public bool dashing;

	[ReadOnly]
	[AllowNesting]
	public bool dashed;

	[ReadOnly]
	[AllowNesting]
	public bool airJumped;

	[ReadOnly]
	[AllowNesting]
	public bool startedDashOnGround;


	public UnityEvent onDash;
}