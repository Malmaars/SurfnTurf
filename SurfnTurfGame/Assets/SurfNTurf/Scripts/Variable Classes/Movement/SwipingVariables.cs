
using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine;
using FMODUnity;

[System.Serializable]
public class SwipingVariables : VariableClass
{
	[SerializeField, Range(0f, 10f)]
	public float swipeRange;

	[SerializeField, Range(0f, 10f)]
	public float swipeDuration;

	[ReadOnly]
	[AllowNesting]
	public float swipeDurationTimer;

	[SerializeField, Range(0f, 100f)]
	public float moveSpeedWhileSwiping;

	[ReadOnly]
	[AllowNesting]
	public bool swiping, swipingOnGround;

	[ReadOnly]
	[AllowNesting]
	public bool desiredSwipe;

	[ReadOnly]
	[AllowNesting]
	public bool swipeAnimation;
	[ReadOnly]
	[AllowNesting]
	public bool doubleJumpAnimation;

	[Header("Sound Refs")]
	public EventReference swipeSound;
	public EventReference swipeWallSound;


}