//using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
public class Gamemanager : MonoBehaviour
{
    PlayerInputActions playerInputActions;
    InputManager playerInputManager;
    public PlayerVFX playerVFX;
    public Transform playerBody;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        playerInputActions = new PlayerInputActions();

        playerInputManager = new InputManager(
            new InputAction[] {
                playerInputActions.Menu.Pause,
                playerInputActions.Menu.Unpause,
                playerInputActions.Menu.Back,
                playerInputActions.Movement.Jump,
                playerInputActions.Movement.DirectionalInput,
                playerInputActions.Movement.Dash,
                playerInputActions.Movement.Look,
                playerInputActions.Movement.Swipe,
                playerInputActions.Movement.Surf,
                playerInputActions.Camera.Rotate,
                playerInputActions.Interactions.Talk,
                playerInputActions.Interactions.Exit,
                playerInputActions.Interactions.ExitPickUp,
                playerInputActions.Movement.OpenCookingStation,
                playerInputActions.Movement.OpenInventoryMenu,
                playerInputActions.Cooking.GoLeft,
                playerInputActions.Cooking.GoRight,
                playerInputActions.Cooking.DirectionalInput,
                playerInputActions.Cooking.Primary,
                playerInputActions.Cooking.Secondary,
                playerInputActions.Prompt.AnyButton,
                playerInputActions.Shop.Next,
                playerInputActions.Shop.Previous,
                playerInputActions.Shop.Buy,
                playerInputActions.Shop.Equip,
                playerInputActions.Challenge.Reset
            });

        InputDistributor.inputManager = playerInputManager;
        InputDistributor.playerInputActions = playerInputActions;
        BlackBoard.playerVFX = playerVFX;
        BlackBoard.playerBody = playerBody;
        BlackBoard.myquests = new Dictionary<string, Quest>();

	}

	private void Start()
	{
	}

	private void OnEnable()
    {
        playerInputManager.WhenEnabled();
    }

    private void OnDisable()
    {
        playerInputManager.WhenDisabled();
    }

}
