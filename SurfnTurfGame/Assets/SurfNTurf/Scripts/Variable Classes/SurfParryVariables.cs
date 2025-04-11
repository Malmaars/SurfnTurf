using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine;

[System.Serializable]
public class SurfParryVariables : VariableClass
{

	public bool goInNormalDirection;

	[ReadOnly]
	[AllowNesting]
	public bool parryIsReady;

	[ReadOnly]
	[AllowNesting]
	public bool dashParryIsReady;
	[ReadOnly]
	[AllowNesting]
	public bool jumpParryIsReady;


	[SerializeField, Range(0f, 10f)]
	public float minimumVelocityToParry;

	[SerializeField, Range(0f, 50f)]
	public float surfParryJumpHeight;

	[SerializeField, Range(0f, 50f)]
	public float dashParryForwardSpeed;
	[SerializeField, Range(0f, 50f)]
	public float dashParryHeight;

	[SerializeField, Range(0f, 2f)]
	public float distanceFromGroundForDashParry;

	[ReadOnly]
	[AllowNesting]
	public bool parryAnimation;

	public UnityEvent OnParry;
}