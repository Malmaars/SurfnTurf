using UnityEngine;
using UnityEngine.InputSystem;

public class PromptState : PlayerState
{
    [SerializeField]private GameObject promptUI;
    public override void EnterState()
    {
        promptUI.SetActive(true);
        enabled = true;

        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Prompt.AnyButton, CheckForInput);
    }
    public override void ExitState()
    {
        promptUI.SetActive(false);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Prompt.AnyButton, CheckForInput);
        base.ExitState();
    }

    public override void InitStateTransitions()
    {
        base.InitStateTransitions();
        transitions.Add(new PlayerStateTransition(typeof(MovementController), () => nextState == typeof(MovementController)));
    }

    public void CheckForInput(InputAction.CallbackContext context)
	{
        nextState = typeof(MovementController);
	}





}
