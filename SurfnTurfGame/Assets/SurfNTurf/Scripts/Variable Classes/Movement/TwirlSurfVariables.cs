using FMOD.Studio;
using FMODUnity;
using NaughtyAttributes;
using Steamworks;
using System;
using UnityEngine;

[Serializable]
public class TwirlSurfVariables : VariableClass
{
	[ReadOnly]
	[AllowNesting]
	public bool twirlSurfing;

	[SerializeField, Range(0f, 10f)]
	public float duration;

	[ReadOnly]
	[AllowNesting]
	public float durationTimer;


	[SerializeField, Range(0f, 20f)]
	public float maximumVelocityMagnitudeForStartBoost;

	[SerializeField, Range(0f, 50f)]
	public float startBoost;

	[SerializeField, Range(0f, 10f)]
	public float minimumVelocityMagnitude;

	[ReadOnly]
	[AllowNesting]
	public bool twirlSurfAnimation;

	[Header("Sound Refs")]
	public EventReference twirlSurfLoop;
	public EventInstance twirlSurfLoopInstance;
}
