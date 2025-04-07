using UnityEngine;

[System.Serializable]
public class SwipeDoubleJumpVariables
{
	public bool jumped;

	[SerializeField, Range(0f, 50f)]
	public float doubleJumpHeight;
}