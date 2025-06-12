using FMOD.Studio;
using FMODUnity;
using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class WaterSurfingVariables : VariableClass
{
	[SerializeField, Range(0f, 1000f)]
	public float accelarationSpeed;

	[SerializeField, Range(0f, 1000f)]
	public float decelerationSpeed;

	[SerializeField, Range(0f, 100f)]
	public float maxSurfSpeed;

	[SerializeField, Range(0f, 50f)]
	public float steeringStrength;

	[ReadOnly]
	[AllowNesting]
	public bool surfing;

	[SerializeField, Range(0f, 90f)]
	public float anglingStrength;

	[ReadOnly]
	[AllowNesting]
	public bool desiredSurf;
	[ReadOnly]
	[AllowNesting]
	public bool onWave;

	[SerializeField, Range(0f, 5f)]
	public float startSurfBuffer;

	[ReadOnly]
	[AllowNesting]
	public float startSurfBufferTimer;

	[SerializeField, Range(0f, 5f)]

	public float surfCooldown;
	[ReadOnly]
	[AllowNesting]
	public float surfCooldownTimer;

	[Header("Sound Refs")]
	public EventReference surfingOnWaterLoop;
	public EventInstance surfingOnWaterLoopInstance;
}
