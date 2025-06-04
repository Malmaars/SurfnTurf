using FMOD.Studio;
using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class TwirlJumpVariables : VariableClass
{
	[ReadOnly]
	[AllowNesting]
	public bool twirlJumping;
	[SerializeField, Range(0f, 50f)]
	public float twirlJumpHeight;
	[SerializeField, Range(-100f, 0f)]
	public float twirlJumpGravityStrength;
	[ReadOnly]
	[AllowNesting]
	public bool turnOffTwirlJump;
	[ReadOnly]
	[AllowNesting]
	public bool twirlJumpAnimation;

	[Header("Sound Refs")]
	public EventReference twirlJumpLoop;
	public EventInstance twirlJumpLoopInstance;

}