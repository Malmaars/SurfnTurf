using UnityEngine;
using UnityEngine.InputSystem;

public class InventoryMenuManager : PlayerState
{
    private bool initialized;
    public GridManager inventory;
    public GameObject hud;

    public override void EnterState()
    {
        gameObject.SetActive(true);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, CloseInventoryMenu);
        Debug.Log("opened menu");
        base.EnterState();
        //cameraController.EnterState();
        //cameraController.SetCamera(1);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (hud != null)
            hud.SetActive(false);
        //ResetGridCursor();
        if (BlackBoard.cookingDatabase.inventoryChanged)
            inventory.LoadIntoGrid(BlackBoard.cookingDatabase.inventoryData);

        //speel animatie van cooking station neerzetten af
    }



    public override void ExitState()
    {
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, CloseInventoryMenu);
        //playerAnimator.SetBool("Table", false);
        //cameraController.ExitState();
        Debug.Log("closed menu");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        base.ExitState();
        if (hud != null)
            hud.SetActive(true);
        BlackBoard.cookingDatabase.SaveInventory(inventory.cells);
        gameObject.SetActive(false);
    }

    private void Awake()
    {
        if (!initialized)
        {

            //cameraController = FindObjectOfType<CookingCameraController>();
            //player = FindObjectOfType<MovementController>().transform;
            initialized = true;
            //gameObject.SetActive(false);
            Debug.Log("test1");
        }
    }

    public void CloseInventoryMenu(InputAction.CallbackContext context)
    {
        BlackBoard.playerManager.SwitchState(typeof(InventoryMenuManager));
    }
}
