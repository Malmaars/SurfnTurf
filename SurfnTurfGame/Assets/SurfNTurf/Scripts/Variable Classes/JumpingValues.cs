using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine;

[System.Serializable]
public class JumpingValues
{
	[SerializeField, Range(0f, 100f)]
	public float jumpHeight = 2f;

	[SerializeField, Range(0, 5)]
	public int maxAirJumps = 0;

	[ReadOnly]
	[AllowNesting]
	public int jumpPhase;

	[ReadOnly]
	[AllowNesting]
	public Vector3 jumpDirection;

	[ReadOnly]
	[AllowNesting]
	public bool desiredJump, jumping, hasLanded, hasLandedAnimation, inAir;

	[SerializeField, Range(0, 5)]
	public float jumpBufferTime;

	[ReadOnly]
	[AllowNesting]
	public float jumpBufferTimer;

	[ReadOnly]
	[AllowNesting]
	public bool jumpBufferActive;

	[SerializeField, Range(0, 2)]
	public float coyoteTime;

	[ReadOnly]
	[AllowNesting]
	public float coyoteTimer;

	[ReadOnly]
	[AllowNesting]
	public bool coyoteTimeAvailable;

	public UnityEvent onJump;
}