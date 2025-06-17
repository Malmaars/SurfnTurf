using FMODUnity;
using NaughtyAttributes;
using UnityEngine;

[System.Serializable]
public class WaterDoubleJumpVariables : VariableClass
{
	public bool jumped;

	[SerializeField, Range(0f, 50f)]
	public float doubleJumpHeight;

	[Header("Sound Refs")]
	public EventReference doubleJumpSound;
}
