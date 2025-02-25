using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;

public class PlayerManager : MonoBehaviour
{
	//manager that only manages large changes with the player. No direct control of movement or anything else
	public List<PlayerState> playerstates;
	
	[SerializeField]
	[ReadOnly]
	PlayerState currentState;

	public PlayerState startState;

	private void Awake()
	{
		currentState = startState;
		currentState.EnterState();
	}
	public void SwitchState(PlayerState newState)
	{
		if (currentState == null)
		{
			currentState = newState;
			currentState.EnterState();
		}

		else
		{
			currentState.ExitState();
			currentState = newState;
			currentState.enabled = true;
			currentState.EnterState();
		}
	}

}
