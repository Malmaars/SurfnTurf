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
	public bool turnOffTwirlJump;
}