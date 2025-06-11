using FMODUnity;
using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class WaterJumpVariables : VariableClass
{
	[ReadOnly]
	[AllowNesting]
	public bool desiredJump;

	[ReadOnly]
	[AllowNesting]
	public bool waterJumping;
	[ReadOnly]
	[AllowNesting]
	public bool didJump;

	[SerializeField, Range(0f, 50f)]
	public float JumpForce;

	[SerializeField, Range(0f, 1f)]
	public float jumpDelay;

	[ReadOnly]
	[AllowNesting]
	public float jumpDelayTimer;

	[Header("Sound Refs")]
	public EventReference jumpOutOfWaterSound;
}
