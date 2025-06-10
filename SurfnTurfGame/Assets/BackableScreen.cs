using Google;
using UnityEngine;
using UnityEngine.InputSystem;

public class BackableScreen : MonoBehaviour
{
    public bool lastScreen;
    public PauseState pauseState;
    public GameObject target;
    public UIManager uiManager;
    public GameObject focus;
    private void OnEnable()
    {
        if (InputDistributor.inputManager == null)
            return;
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Back, GoBack);
    }

    private void OnDisable()
    {
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Back, GoBack);
    }

    public void GoBack(InputAction.CallbackContext context)
    {
        if (lastScreen)
        {
            //exit out of the pause state
            pauseState.SwitchButton();
            return;
        }

        if (target == null)
            return;
        target.SetActive(true);
        
        uiManager.SetFocus(focus);
        gameObject.SetActive(false);
    }
}
