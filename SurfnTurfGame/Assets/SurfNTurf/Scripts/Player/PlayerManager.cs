using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using NaughtyAttributes;
using NaughtyAttributes.Test;
using Unity.VisualScripting;
using System;
using UnityEngine.InputSystem.LowLevel;

public class PlayerManager : MonoBehaviour
{
	//manager that only manages large changes with the player. No direct control of movement or anything else
	public List<PlayerState> playerstates;
	
	[SerializeField]
	[ReadOnly]
	PlayerState currentState;
	PlayerState previousState;

	public PlayerState startState;

	private void Awake()
	{
		BlackBoard.playerManager = this;
		
	}
	private void Start()
	{
		//find all player states components in scene and add make a new list with them
		playerstates = new List<PlayerState>(FindObjectsByType<PlayerState>(FindObjectsSortMode.InstanceID));

		currentState = startState;
		currentState.EnterState();

		foreach(PlayerState p in playerstates)
		{
			p.Initialize();
			p.InitStateTransitions();
			if (p != currentState)
				p.ExitState();
		}
	}

	private void Update()
	{
		currentState.TransitionUpdate();
	}

    public void SwitchState(int newStateIndex) { SwitchState(playerstates[newStateIndex].GetType()); }

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

		if(newState == null)
		{
			Debug.LogError("Playerstate does not exist");
			return;
		}

		SwitchState(newState);
	}

	public void SwitchState(PlayerState newState)
	{

		if (newState == null)
			return;

		if (currentState == null)
		{
			currentState = newState;
			currentState.EnterState();
		}

		else
		{
			if (previousState != currentState)
				previousState = currentState;

			currentState.ExitState();
			currentState = newState;
			currentState.enabled = true;
			currentState.EnterState();
		}
	}

	public void SwitchToPreviousState()
	{
		if(previousState == null)
		{
			Debug.LogError("No previous State");
			return;
		}

		SwitchState(previousState);
	}

	public PlayerState GetCurrentState()
	{
		return currentState;
	}

}
