using System;
using UnityEngine;
using NaughtyAttributes;

[Serializable]
public class BoardtrickVariables : VariableClass
{
	[SerializeField, Range(0f, 40f)]
	public float kickflipHeight;

	[SerializeField, Range(0f, 5f)]
	public float kickflipCooldown;

	[ReadOnly]
	[AllowNesting]
	public float kickflipCooldownTimer;	
	
	[SerializeField, Range(0f, 40f)]
	public float shoveItHeight;

	[SerializeField, Range(0f, 5f)]
	public float shoveItCooldown;

	[ReadOnly]
	[AllowNesting]
	public float shoveItCooldownTimer;
}
