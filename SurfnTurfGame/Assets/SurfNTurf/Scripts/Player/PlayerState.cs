using System.Collections.Generic;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class PlayerState : MonoBehaviour
{
    public List<PlayerStateTransition> transitions { get; protected set; }
    protected Type nextState = null;

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
                transition.SpecialExit.Invoke();
                BlackBoard.playerManager.SwitchState(transition.target);
                break;
            }
        }
    }

    public virtual void EnterState() { }

    public virtual void InitStateTransitions()
    {
        transitions = new List<PlayerStateTransition>();
    }
}

public delegate bool Condition();

public class PlayerStateTransition
{
    public System.Type target;
    public Condition condition;

    public PlayerStateTransition(System.Type _target, Condition _condition)
    {
        target = _target;
        condition = _condition;
    }

    public UnityEvent SpecialExit;
}
