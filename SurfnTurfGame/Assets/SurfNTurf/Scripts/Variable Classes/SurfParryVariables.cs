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

	[SerializeField, Range(0f, 10f)]
	public float minimumVelocityToParry;


	[SerializeField, Range(0f, 50f)]
	public float surfParryJumpHeight;

	public UnityEvent OnParry;
}