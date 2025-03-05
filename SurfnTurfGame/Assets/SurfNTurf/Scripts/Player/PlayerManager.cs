using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using NaughtyAttributes.Test;
using Unity.VisualScripting;
using System;

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
		BlackBoard.playerManager = this;
	}
	private void Start()
	{
		currentState = startState;
		currentState.EnterState();

		foreach(PlayerState p in playerstates)
		{
			if (p != currentState)
				p.ExitState();
		}
	}

	public void SwitchState(int newStateIndex)
	{
		if (currentState == null)
		{
			currentState = playerstates[newStateIndex];
			currentState.EnterState();
		}

		else
		{
			currentState.ExitState();
			currentState = playerstates[newStateIndex];
			currentState.enabled = true;
			currentState.EnterState();
		}
	}

	public void SwitchState(Type switchType)
	{
		PlayerState newState = null;
		foreach(PlayerState ps in playerstates)
		{
			if (ps.GetType() == switchType)
			{
				newState = ps; 
				break;
			}
		}

		if (newState == null)
			return;

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
