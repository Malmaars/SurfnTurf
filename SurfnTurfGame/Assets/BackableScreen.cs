using Google;
using UnityEngine;
using UnityEngine.InputSystem;

public class BackableScreen : Initializable
{
    public bool lastScreen;
    public PauseState pauseState;
    public GameObject target;
    public UIManager uiManager;
    public GameObject focus;

    public override void Enterialize()
    {
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Back, GoBack);
    }
    private void OnEnable()
    {
        Initialize();
    }

    private void OnDisable()
    {
        Exitialize();
    }

    public override void Exitialize()
    {
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Back, GoBack);
    }

    public void GoBack(InputAction.CallbackContext context)
    {
        if (lastScreen)
        {
            //exit out of the pause state
            pauseState.SwitchButton();
            InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Back, GoBack);
            return;
        }

        if (target == null)
            return;
        target.SetActive(true);
        
        uiManager.SetFocus(focus);
        gameObject.SetActive(false);
    }
}
