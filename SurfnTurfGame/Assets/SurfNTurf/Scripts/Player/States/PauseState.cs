using FMODUnity;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PauseState : PlayerState
{
    public GameObject pauseScreen;

    public CinemachineCamera UIcam;

    public Animator pauseAnimator;
    public GameObject creditScreen;
    public GameObject focusButton;
    public GameObject focusCreditsButton;
    public GameObject SettingsScreen;
    public EventReference PauseMenuOpenEventReference;

    public Initializable[] Initializables;
    public override void Initialize()
    {
        //Initializables = GetComponentsInChildren<Initializable>();
        foreach(Initializable i in Initializables) 
        {
            i.Initialize();
        }
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

        if (UIcam != null)
        {
            UIcam.transform.position = BlackBoard.cameraController.currentCamera.transform.position;
            UIcam.transform.rotation = BlackBoard.cameraController.currentCamera.transform.rotation;
        }
        BlackBoard.cameraController.SwitchToCamera(UIcam);

        foreach (Initializable i in Initializables)
        {
            i.Enterialize();
        }
        UIManager.instance.LoadVsync();
    }

    public void SwitchBack(InputAction.CallbackContext context)
    {
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

        foreach (Initializable i in Initializables)
        {
            i.Exitialize();
        }
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

