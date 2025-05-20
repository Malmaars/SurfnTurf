using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class MainMenuState : PlayerState
{
    [SerializeField] CinemachineCamera MainMenuCamera;
    [SerializeField] Animator logoAnimator;
    [SerializeField] Canvas mainMenuUI;
    public override void EnterState()
    {
        mainMenuUI.enabled = true;
        UIManager.instance.SetVisibleUI(false);
        logoAnimator.gameObject.SetActive(false);
        Invoke("StartAnimation", 1f);
        BlackBoard.cameraController.SwitchToCamera(MainMenuCamera, 0f);
        enabled = true;

        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Prompt.AnyButton, CheckForInput);
        base.EnterState();
    }
    public override void ExitState()
    {
        mainMenuUI.enabled = false;
        logoAnimator.gameObject.SetActive(false);
        logoAnimator.SetBool("Start", false);
        UIManager.instance.SetVisibleUI(true);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Prompt.AnyButton, CheckForInput);
        base.ExitState();
    }

    public override void InitStateTransitions()
    {
        base.InitStateTransitions();
        transitions.Add(new PlayerStateTransition(typeof(MovementController), () => nextState == typeof(MovementController)));
        transitions.Add(new PlayerStateTransition(typeof(PauseState), () => nextState == typeof(PauseState)));
    }

    public void CheckForInput(InputAction.CallbackContext context)
    {
        nextState = typeof(MovementController);
    }
    
    private void StartAnimation()
    {
        logoAnimator.gameObject.SetActive(true);
        logoAnimator.SetBool("Start", true);
    }
}
