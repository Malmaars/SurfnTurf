//using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gamemanager : MonoBehaviour
{
    PlayerInputActions playerInputActions;
    InputManager playerInputManager;
    public PlayerVFX playerVFX;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerInputActions = new PlayerInputActions();

        playerInputManager = new InputManager(
            new InputAction[] {
                playerInputActions.Menu.Pause,
                playerInputActions.Menu.Unpause,
                playerInputActions.Movement.Jump,
                playerInputActions.Movement.DirectionalInput,
                playerInputActions.Movement.Dash,
                playerInputActions.Movement.Look,
                playerInputActions.Movement.Swipe,
                playerInputActions.Movement.Surf,
                playerInputActions.Camera.Rotate,
                playerInputActions.Interactions.Talk,
                playerInputActions.Interactions.Exit,
                playerInputActions.Movement.OpenCookingStation,
                playerInputActions.Movement.OpenInventoryMenu,
                playerInputActions.Cooking.GoLeft,
                playerInputActions.Cooking.GoRight,
                playerInputActions.Cooking.DirectionalInput,
                playerInputActions.Prompt.AnyButton
            });

        InputDistributor.inputManager = playerInputManager;
        InputDistributor.playerInputActions = playerInputActions;
        BlackBoard.playerVFX = playerVFX;
    }

    private void OnEnable()
    {
        playerInputManager.WhenEnabled();
    }

    private void OnDisable()
    {
        playerInputManager.WhenDisabled();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
