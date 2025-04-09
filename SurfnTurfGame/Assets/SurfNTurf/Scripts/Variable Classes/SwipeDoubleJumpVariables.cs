using UnityEngine;

[System.Serializable]
public class SwipeDoubleJumpVariables : VariableClass
{
	public bool jumped;

	[SerializeField, Range(0f, 50f)]
	public float doubleJumpHeight;
}