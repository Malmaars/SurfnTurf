using NaughtyAttributes;
using System;
using UnityEngine;

[Serializable]
public class GrindVariables : VariableClass
{
	[SerializeField, Range(0f, 3f)]
	public float offsetFromPlayer;

	[SerializeField, Range(0f, 3f)]
	public float offsetFromPole;

	[ReadOnly]
	[AllowNesting]
	public float grindSpeed;

	[SerializeField, Range(0f, 100f)]
	public float baseGrindSpeed;

	[SerializeField, Range(0f, 20f)]
	public float maxGrindSpeed;


	[SerializeField, Range(0f, 10f)]
	public float grindDamping;
	
	[SerializeField, Range(0f, 10f)]
	public float grindSpeedUp;


	[SerializeField, Range(0f, 4f)]
	public float checkSize;

	[ReadOnly]
	[AllowNesting]
    public bool grinding;
	
	[ReadOnly]
	[AllowNesting]
    public bool grindGravityOff;

	[ReadOnly]
	[AllowNesting]
	public Transform currentRail;

	[ReadOnly]
	[AllowNesting]
	public Vector3 grindDirection;
}
