using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using System.Collections;
using Unity.Cinemachine;
using Steamworks;

public class InventoryMenuManager : PlayerState
{
    private bool initialized;
    public GameObject hud;

    public GridManager inventory;
    public PieceHolder pieceHolder;
    public PieceManager pieceManager;
    public Transform pieceAnimationHelper;
    public bool isHoldingSomething;
    public bool isPlayingAnimation;
    public float rotationDuration = 0.25f;

    public GridCursor gridCursor;
    private bool gridCursorSet;
    public float cursorSpeedupTime;
    public float cursorSpeedupTimer;

    public LayerMask gridLayers;

    [SerializeField]
    private float offGridDistance = 10;
    [SerializeField]
    private float aboveGridDistance = 1;

    public CinemachineCamera inventoryMenuCamera;
    public Transform inventoryMenuCameraPivot;

    private Vector3 previousMousePosition;
    private Vector3 offGridPosition;
    private Quaternion offGridRotation;
    private Vector3 aboveGridPosition;
    private Vector2Int onGridPosition;

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
        ResetGridCursor();
        inventoryMenuCamera.transform.position = inventoryMenuCameraPivot.position;
        inventoryMenuCamera.transform.rotation = inventoryMenuCameraPivot.rotation;
        BlackBoard.cameraController.SwitchToCamera(inventoryMenuCamera, 0.2f);
        StartCoroutine(SetSteamCounterStat("time_spent_cooking"));
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
        StopCoroutine(SetSteamCounterStat("time_spent_cooking"));
        gameObject.SetActive(false);

    }

    private void Awake()
    {
        if (!initialized)
        {

            //cameraController = FindObjectOfType<CookingCameraController>();
            //player = FindObjectOfType<MovementController>().transform;
            inventory.ActivateGrid(0);
            initialized = true;
            //gameObject.SetActive(false);
            Debug.Log("test1");
        }
    }

    private void Update()
    {

        if (isHoldingSomething)
        {
            if (!isPlayingAnimation)
            {
                if (Input.mouseScrollDelta.y >= 1)
                {
                    StartCoroutine(RotatePiece(true));
                }
                else if (Input.mouseScrollDelta.y <= -1)
                {
                    StartCoroutine(RotatePiece(false));
                }

                if (Input.GetMouseButtonDown(0))
                {
                    if (CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, inventory))
                    {
                        pieceManager.SetPiece(inventory, onGridPosition);
                        isHoldingSomething = false;
                        Cursor.visible = true;
                    }
                    if (inventory.extractWhole == false)
                    {
                        FoodCell selectedCell = CookingHelperFunctions.PieceCompatible(pieceManager.cells, onGridPosition, inventory);
                        if (selectedCell != null)
                        {
                            pieceManager.SwapPieces(inventory, selectedCell, onGridPosition);
                            pieceHolder.transform.position = pieceManager.originalCenterPosition;
                            HandleMouseVisual();
                        }
                    }
                }
            }
            if (CollidingWithGrid())
            {
                if (CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, inventory))
                {
                    pieceHolder.MoveObjectToGrid(onGridPosition, inventory, pieceManager.pieceCenterOffset, inventory.cellScale);
                }
                else
                {
                    pieceHolder.MoveObjectAboveGrid(aboveGridPosition, inventory.transform.rotation);
                }
            }
            else
            {
                CalculateOffGridPosition();
                pieceHolder.MoveObjectToPoint(offGridPosition, offGridRotation);
            }
        }
        else
        {
            if (inventory != null)
                pieceHolder.MoveObjectAboveGrid(aboveGridPosition, inventory.transform.rotation);
            if (CollidingWithGrid() && Input.GetMouseButtonDown(0))
            {
                pieceManager.ExtractPiece(inventory, onGridPosition);
                if (pieceManager.cells.Count > 0)
                {
                    isHoldingSomething = true;
                    pieceHolder.transform.position = pieceManager.originalCenterPosition;
                    HandleMouseVisual();
                }
            }
        }
        if (previousMousePosition != Input.mousePosition)
        {
            if (gridCursor.visible)
            {
                gridCursor.Visible(false);
                Cursor.visible = true;
            }
        }
        else
        {
            MoveGridCursor();
        }
        previousMousePosition = Input.mousePosition;
    }

    public void CloseInventoryMenu(InputAction.CallbackContext context)
    {
        BlackBoard.playerManager.SwitchState(typeof(MovementController));
    }

    public void HandleMouseVisual()
    {
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(pieceManager.originalCenterPosition);
        Mouse.current.WarpCursorPosition(screenPoint);
        Cursor.visible = false;
    }

    public IEnumerator RotatePiece(bool clockwise)
    {
        isPlayingAnimation = true;

        foreach (FoodCell cell in pieceManager.cells)
        {
            cell.SetParent(pieceAnimationHelper, false);
        }

        float rotationAngle = clockwise ? -90f : 90f;
        float elapsedTime = 0f;
        Quaternion targetRotation = quaternion.identity * Quaternion.Euler(0, 0, rotationAngle);

        while (elapsedTime < rotationDuration)
        {
            pieceAnimationHelper.localRotation = Quaternion.Lerp(quaternion.identity, targetRotation, elapsedTime / rotationDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        pieceAnimationHelper.localRotation = quaternion.identity;

        pieceManager.RotatePiece(clockwise);

        isPlayingAnimation = false;
    }

    private void CalculateOffGridPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        offGridPosition = ray.origin + ray.direction.normalized * offGridDistance;
        offGridRotation = Quaternion.LookRotation(ray.direction);
    }

    private void ResetGridCursor()
    {
        //set the grid to the center of the first grid in list
        onGridPosition.x = (int)inventory.gridSize.x / 2;
        onGridPosition.y = (int)inventory.gridSize.y / 2;
        if(gridCursor != null)
        {
            gridCursor.SetPosition(inventory.gridPositions[onGridPosition.x, onGridPosition.y].transform, true);
            gridCursor.Visible(false);
            gridCursorSet = true;
        }
    }

    private void MoveGridCursor()
    {
        
        Vector2Int playerInput = Vector2Int.RoundToInt(InputDistributor.playerInputActions.Cooking.DirectionalInput.ReadValue<Vector2>());

        if (playerInput == Vector2Int.zero)
        {
            cursorSpeedupTimer = cursorSpeedupTime;
            return;
        }

        cursorSpeedupTimer -= Time.deltaTime;

        if (!gridCursor.canMove)
            return;

        if (!gridCursor.visible)
        {
            gridCursor.Visible(true);
            Cursor.visible = false;
        }

        Vector2Int newPos = onGridPosition + playerInput;
        Vector2Int finalPos = newPos;

        if (newPos.x < 0)
        {
            finalPos.x = onGridPosition.x;
        }
        if (newPos.x >= inventory.gridSize.x)
        {
            finalPos.x = onGridPosition.x;
        }
        if (newPos.y < 0 || newPos.y >= inventory.gridSize.y)
        {
            finalPos.y = onGridPosition.y;
        }

        bool moveFast = cursorSpeedupTimer < 0;
        gridCursor.SetPosition(inventory.gridPositions[finalPos.x, finalPos.y], moveFast);
        onGridPosition = finalPos;

        if (gridCursor.visible == false)
            gridCursor.Visible(true);
    }

    private bool CollidingWithGrid()
    {
        if (previousMousePosition == Input.mousePosition && inventory != null)
        {
            return true;
        }
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, gridLayers))
        {
            if (hit.transform.tag == "Grid")
            {
                Vector3 mouseDirection = (hit.point - Camera.main.ScreenToWorldPoint(Input.mousePosition)).normalized;
                aboveGridPosition = hit.point - mouseDirection * aboveGridDistance;
                onGridPosition = CookingHelperFunctions.ConvertPointToGrid(hit.point, hit.transform, pieceManager.pieceCenterOffset, inventory.cellScale);
                return true;
            }
        }
        return false;
    }
    public IEnumerator SetSteamCounterStat(string statName)
    {
        if (SteamManager.Initialized)
        {
            SteamUserStats.GetStat(statName, out int statValue);
            statValue++;
            SteamUserStats.SetStat(statName, statValue);
            SteamUserStats.StoreStats();
        }
        yield return new WaitForSeconds(1);
        SetSteamCounterStat(statName);
    }
}
