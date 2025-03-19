using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PauseState : PlayerState
{
    bool paused = false;

    public GameObject pauseScreen;

    public Animator pauseAnimator;

    public GameObject creditScreen;
    public UIManager uiManager;

    public override void EnterState()
    {
        base.EnterState();

        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Unpause, SwitchBack);

        if (!pauseScreen.activeInHierarchy) pauseScreen.SetActive(true);
        else pauseAnimator.Play("Pause");
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        uiManager.LoadVsync();
    }

    void SwitchBack(InputAction.CallbackContext context)
    {
        //Go back to a specific state
        BlackBoard.playerManager.SwitchToPreviousState();
    }

    public override void ExitState()
    {

        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Unpause, SwitchBack);

        pauseAnimator.Play("Unpause");
        creditScreen.SetActive(false);
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        uiManager.LoadVsync();
        base.ExitState();
    }
}
