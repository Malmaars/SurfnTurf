//using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;

public class Gamemanager : MonoBehaviour
{
    PlayerInputActions playerInputActions;
    InputManager playerInputManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerInputActions = new PlayerInputActions();

        playerInputManager = new InputManager(
            new InputAction[] {
                playerInputActions.Movement.Jump,
                playerInputActions.Movement.DirectionalInput,
                playerInputActions.Movement.Dash,
                playerInputActions.Camera.Rotate,
                playerInputActions.Interactions.Talk,
                playerInputActions.Movement.OpenCookingStation,
                playerInputActions.Cooking.GoLeft,
                playerInputActions.Cooking.GoRight,
                playerInputActions.Cooking.DirectionalInput
            });

        InputDistributor.inputManager = playerInputManager;
        InputDistributor.playerInputActions = playerInputActions;
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
