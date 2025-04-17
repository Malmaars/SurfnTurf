using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class LedgeGrabVariables : VariableClass
{

	public LayerMask ledgeGrabbable;

	[ReadOnly]
	[AllowNesting] 
	public bool ledgeGrabbing;

	public Vector2 teleportoffset;
	[SerializeField, Range(0f, 5f)]
	public float maxDistanceForward;

	//the height offset from the player where the raycast starts
	[SerializeField, Range(0f, 5f)]
	public float heightToCast;

	[SerializeField, Range(0f, 5f)]
	public float raycastDistance;

	[SerializeField, Range(0f, 1f)]
	public float heightLeeway;


	[SerializeField, Range(0f, 5f)]
	public float ledgeGrabDuration;

	[ReadOnly]
	[AllowNesting]
	public float ledgeGrabDurationTimer;

	[ReadOnly]
	[AllowNesting]
	public bool ledgeGrabAnimation;
	[ReadOnly]
	[AllowNesting]
	public bool startedAnimation;

	[ReadOnly]
	[AllowNesting]
	public bool turnGravityOff;


}
