using FMOD.Studio;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class GroundSurfVariables : VariableClass
{
	[SerializeField, Range(0f, 1f)]
	public float groundNormalThreshold;

	[SerializeField, Range(0f, 1000f)]
	public float accelarationSpeed;

	[SerializeField, Range(0f, 1000f)]
	public float decelerationSpeed;

	[SerializeField, Range(0f, 100f)]
	public float maxSurfSpeed;

	[SerializeField, Range(0f, 10f)]
	public float steeringStrength;

	[ReadOnly]
	[AllowNesting]
	public bool surfing;

	[ReadOnly]
	[AllowNesting]
	public bool desiredSurf;

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
	public EventReference surfingOnLandLoop;
	public EventInstance surfingOnLandLoopInstance;
}