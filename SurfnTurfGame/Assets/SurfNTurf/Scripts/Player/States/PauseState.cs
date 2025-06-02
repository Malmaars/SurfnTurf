using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseState : PlayerState
{
    public GameObject pauseScreen;

    public Animator pauseAnimator;
    public GameObject creditScreen;
    public GameObject focusButton;
    public GameObject focusCreditsButton;
    public GameObject SettingsScreen;
    public EventReference PauseMenuOpenEventReference;
    public override void Initialize()
    {
        SettingsScreen.SetActive(false);
    }

    public override void EnterState()
    {
        base.EnterState();
        pauseScreen.SetActive(true);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Unpause, SwitchBack);
        focusUI(focusButton);
        if (!pauseScreen.activeInHierarchy) pauseScreen.SetActive(true);
        else pauseAnimator.Play("Pause");
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        UIManager.instance.LoadVsync();
    }

    public void SwitchBack(InputAction.CallbackContext context)
    {
        //Go back to a specific state
        if (UIManager.instance.pickUpUI.uIActive) return;
        //play fmod sound
        RuntimeManager.PlayOneShot(PauseMenuOpenEventReference);
        SwitchButton();
    }


    public void SwitchButton()
    {
        BlackBoard.playerManager.SwitchToPreviousState();
    }

    public override void ExitState()
    {

        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Unpause, SwitchBack);

        pauseAnimator.Play("Unpause");
        creditScreen.SetActive(false);
        UnfocusAllUI();
        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        UIManager.instance.LoadVsync();
        base.ExitState();
    }
    public void UnfocusAllUI()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void focusUI(GameObject gameObject)
    {
        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void OpenCredits()
    {
        creditScreen.SetActive(true);
        pauseScreen.SetActive(false);
        ;
        focusUI(focusCreditsButton);
    }
}

