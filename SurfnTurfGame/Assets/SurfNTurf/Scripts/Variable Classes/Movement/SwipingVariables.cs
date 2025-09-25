
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
	[SerializeField, Range(0f, 10f)]
	public float swipeComboWindow = 0.2f;

	[ReadOnly]
	[AllowNesting]
	public float swipeDurationTimer;
	[ReadOnly]
	[AllowNesting]
	public int swipeComboIndex = 0;

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
	public bool swipeSecondAnimation;
	[ReadOnly]
	[AllowNesting]
	public bool swipeThirdAnimation;
	[ReadOnly]
	[AllowNesting]
	public bool doubleJumpAnimation;

	[Header("Sound Refs")]
	public EventReference swipeSound;
	public EventReference swipeWallSound;


}