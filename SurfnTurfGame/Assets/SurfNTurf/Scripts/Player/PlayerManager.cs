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

	// Static field to hold the instance
	private static PlayerManager _instance;

	// Property to get the instance
	public static PlayerManager Instance
	{
		get
		{
			if (_instance == null)
			{
				// Try to find the instance in the scene
				_instance = FindFirstObjectByType<PlayerManager>();

				// If no instance found, you can log a warning
				if (_instance == null)
				{
					Debug.LogWarning("Playermanager instance not found in the scene!");
				}
			}
			return _instance;
		}
	}


	private void Awake()
	{
		// If an instance already exists and it's not this one, destroy this object
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			// Otherwise, set the instance to this object
			_instance = this;
			DontDestroyOnLoad(gameObject); // Optional: persists across scene loads
		}

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
