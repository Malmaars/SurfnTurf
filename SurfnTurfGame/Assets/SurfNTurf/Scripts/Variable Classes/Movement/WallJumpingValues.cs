using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine;
using FMODUnity;

[System.Serializable]
public class WallJumpingValues : VariableClass
{
	[ReadOnly]
	[AllowNesting]
	public Vector3 jumpDirection;

	[SerializeField, Range(4, 64)]
	public int wallRaycastAmount = 4;

	[SerializeField, Range(0, 4)]
	public float distanceUntilWallGrab = 1;
	[SerializeField, Range(0, 4)]
	public float minimumDistanceFromGround = 1;

	[SerializeField, Range(0, 1)]
	public float maxWallAngleOffsetZeroToOne, inputDirectionLeeway = 0.2f, wallRidingMinimumOffset;

	[SerializeField, Range(-30, 0)]
	public float wallGrabGravity = -3f;

	[SerializeField, Range(0f, 100f)]
	public float wallJumpForce = 2f, wallRidingJumpForce = 2f;

	[SerializeField, Range(0f, 90f)]
	public float walljumpAngle = 45f;

	[SerializeField, Range(0f, 10f)]
	public float wallJumpAntiAirTimer = 0.5f;

	[SerializeField, Range(0f, 1f)]
	public float wallJumpCooldown = 0.5f;

	[SerializeField, Range(0f, 2f)]
	public float wallJumpCoyoteTime = 0.2f;

	[ReadOnly]
	[AllowNesting]
	public float wallJumpCoyoteTimer;

	[SerializeField, Range(0f, 2f)]
	public float maxHeight = 0.2f;
	[SerializeField, Range(0f, 2f)]
	public float maxHeightRayLength = 1f;


	[ReadOnly]
	[AllowNesting]
	public bool wallJumpLimitVelocity;

	[ReadOnly]
	[AllowNesting]
	public float wallJumpCooldownTimer;

	[ReadOnly]
	[AllowNesting]
	public bool wallgrab, wallRiding, wallJumped;

	[ReadOnly]
	[AllowNesting]
	public bool wallgrabAnimation;

	[ReadOnly]
	[AllowNesting]
	public bool wallJumpAnimation;


	[ReadOnly]
	[AllowNesting]
	public Vector3 currentWallNormal;

	[Header("Sound Refs")]
	public EventReference wallJumpSound;
}