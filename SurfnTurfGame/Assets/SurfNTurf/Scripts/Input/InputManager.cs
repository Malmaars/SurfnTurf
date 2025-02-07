using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager
{

    InputAction[] actions;

    Dictionary<InputAction, List<Action<InputAction.CallbackContext>>> inputsAndActions;

    /// <summary>
    /// When initializing this, define which actions you want to use. It should be an array of InputActions with every action you want to use
    /// </summary>
    public InputManager(InputAction[] _actions)
    {
        actions = _actions;
        inputsAndActions = new Dictionary<InputAction, List<Action<InputAction.CallbackContext>>>();

        foreach (InputAction action in actions)
        {
            inputsAndActions.Add(action, new List<Action<InputAction.CallbackContext>>());
        }
    }

    /// <summary>
    /// Run this in OnEnable to make sure every inputAction is enabled
    /// </summary>
    public void WhenEnabled()
    {
        foreach (InputAction action in actions)
        {
            action.Enable();
        }
    }

    /// <summary>
    /// Run this in OnDisable to make sure every inputAction is properly disabled
    /// </summary>
    public void WhenDisabled()
    {
        foreach (InputAction action in actions)
        {
            action.Disable();
            //ClearAllActionsFromInput(action);
        }
    }

    /// <summary>
    /// Add an action to an existing input type
    /// </summary>
    /// <param name="inputToAddTo">input to add the action to</param>
    /// <param name="actionToAdd">action to add to the input</param>

    public void AddActionToInput(InputAction inputToAddTo, Action<InputAction.CallbackContext> actionToAdd)
    {
        inputToAddTo.performed += actionToAdd;
        inputsAndActions[inputToAddTo].Add(actionToAdd);
    }

    /// <summary>
    /// Add an action to the cancellation of an action, to be used in tandem with AddActionToInput()
    /// </summary>
    /// <param name="inputToAddTo">input to add the action to</param>
    /// <param name="actionToAdd">action to add to the input</param>

    public void AddActionToInputCancelled(InputAction inputToAddTo, Action<InputAction.CallbackContext> actionToAdd)
    {
        inputToAddTo.canceled += actionToAdd;
    }

    /// <summary>
    /// Remove an action from an existing input type
    /// </summary>
    /// <param name="inputToRemoveFrom">input to remove the action from</param>
    /// <param name="actionToRemove">action to remove from the input</param>
    public void RemoveActionFromInput(InputAction inputToRemoveFrom, Action<InputAction.CallbackContext> actionToRemove)
    {
        inputToRemoveFrom.performed -= actionToRemove;
        inputsAndActions[inputToRemoveFrom].Remove(actionToRemove);
    }

    /// <summary>
    /// Clears all added Actions from a specific InputAction
    /// </summary>
    /// <param name="inputToRemoveFrom">input from which you want to remove all actions</param>
    public void ClearAllActionsFromInput(InputAction inputToRemoveFrom)
    {
        foreach (Action<InputAction.CallbackContext> action in inputsAndActions[inputToRemoveFrom])
        {
            inputToRemoveFrom.performed -= action;
        }

        inputsAndActions[inputToRemoveFrom].Clear();
    }
}
