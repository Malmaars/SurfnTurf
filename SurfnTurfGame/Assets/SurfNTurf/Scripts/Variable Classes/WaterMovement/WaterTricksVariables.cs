using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class WaterTricksVariables : VariableClass
{
	[SerializeField, Range(0f, 40f)]
	public float kickflipHeight;

	[SerializeField, Range(0f, 5f)]
	public float kickflipCooldown;

	public int kickFlipTokensFromGround, extraKickFlipTokens;

	[ReadOnly]
	[AllowNesting]
	public int activeKickFlipTokens;

	[ReadOnly]
	[AllowNesting]
	public float kickflipCooldownTimer;

	[ReadOnly]
	[AllowNesting]
	public bool kickFlipAnimation;

	[SerializeField, Range(0f, 40f)]
	public float shoveItHeight;

	public int shoveItTokensFromGround, extraShoveItTokens;

	[ReadOnly]
	[AllowNesting]
	public int activeShoveItTokens;

	[SerializeField, Range(0f, 5f)]
	public float shoveItCooldown;

	[ReadOnly]
	[AllowNesting]
	public float shoveItCooldownTimer;

	[ReadOnly]
	[AllowNesting]
	public bool shoveItAnimation;
}
