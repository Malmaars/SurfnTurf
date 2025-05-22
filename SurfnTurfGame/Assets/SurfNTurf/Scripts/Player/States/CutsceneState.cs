using UnityEngine;

public class CutsceneState : PlayerState
{
    public override void InitStateTransitions()
    {
        base.InitStateTransitions();
        transitions.Add(new PlayerStateTransition(typeof(PauseState), () => nextState == typeof(PauseState)));
        transitions.Add(new PlayerStateTransition(typeof(MovementController), () => nextState == typeof(MovementController)));
    }

    public override void EnterState()
    {
        base.EnterState();
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);

    }

    public override void ExitState()
    {
        base.ExitState();
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
    }
}
