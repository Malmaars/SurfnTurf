using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerState : MonoBehaviour
{
    public List<PlayerStateTransition> transitions { get; protected set; }
    protected Type nextState = null;

	public virtual void EnterState() { }
	public virtual void ExitState()
	{
        nextState = null;
		enabled = false;
	}

    public virtual void TransitionUpdate()
    {
        foreach(PlayerStateTransition transition in transitions)
        {
            if(transition.condition())
            {

                for(int i = 0; i < transition.exitCalls.Length; i++)
                {
                    transition.exitCalls[i]();
                }

                BlackBoard.playerManager.SwitchState(transition.target);
                break;
            }
        }
    }

    public virtual void InitStateTransitions()
    {
        transitions = new List<PlayerStateTransition>();
    }

    public void PauseGame(InputAction.CallbackContext context)
    {
        nextState = typeof(PauseState);
    }

}

public delegate bool Condition();
public delegate void specialExit();

public class PlayerStateTransition
{
    public System.Type target;
    public Condition condition;
    public specialExit[] exitCalls;

    public PlayerStateTransition(System.Type _target, Condition _condition)
    {
        target = _target;
        condition = _condition;
        exitCalls = new specialExit[0];
    }
    public PlayerStateTransition(System.Type _target, Condition _condition, specialExit[] _onExit)
    {
        target = _target;
        condition = _condition;
        exitCalls = _onExit;
    }
}
