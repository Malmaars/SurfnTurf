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
    public Camera inventoryCamera;

    public GridManager inventory;
    public PieceHolder pieceHolder;
    public PieceManager pieceManager;
    public Transform pieceAnimationHelper;
    public bool isHoldingSomething;
    public bool isPlayingAnimation;
    public float rotationDuration = 0.25f;
    public bool isCollidingWithGrid;
    [NaughtyAttributes.ReadOnly] public bool isHoldingPiece;

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
    private Vector3 worldPosition;
    private FoodCell currentSelectedCell;

    public override void InitStateTransitions()
    {
        base.InitStateTransitions();
        transitions.Add(new PlayerStateTransition(typeof(PauseState), () => nextState == typeof(PauseState)));
        transitions.Add(new PlayerStateTransition(typeof(MovementController), () => nextState == typeof(MovementController)));
    }

    public override void EnterState()
    {
        gameObject.SetActive(true);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, CloseInventoryMenu);
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
        //ResetGridCursor();
        inventoryMenuCamera.transform.position = inventoryMenuCameraPivot.position;
        inventoryMenuCamera.transform.rotation = inventoryMenuCameraPivot.rotation;
        BlackBoard.cameraController.SwitchToCamera(inventoryMenuCamera, 0.2f);
        StartCoroutine(SetSteamCounterStat("time_spent_cooking"));
        //speel animatie van cooking station neerzetten af
    }



    public override void ExitState()
    {
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, CloseInventoryMenu);
        //playerAnimator.SetBool("Table", false);
        //cameraController.ExitState();
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
        RetrieveReferences();
        inventory.ActivateGrid(0);
    }

    private void RetrieveReferences()
    {
        pieceHolder = transform.GetChild(0).GetComponent<PieceHolder>();
        pieceManager = transform.GetChild(0).GetComponent<PieceManager>();
        pieceAnimationHelper = transform.GetChild(0).GetChild(0);

        gridCursor = transform.GetChild(1).GetComponent<GridCursor>();

        inventory = GetComponentInChildren<Inventory>();
    }

    public void CloseInventoryMenu(InputAction.CallbackContext context)
    {
        nextState = typeof(MovementController);
    }

    private void Update()
    {
        isCollidingWithGrid = CollidingWithGrid();
        //HandleTooltip();

        if (isHoldingPiece)
        {
            HandlePieceHolder();

            if (HandlePieceRotation() && isCollidingWithGrid && Input.GetMouseButtonDown(0))
            {
                HandlePiecePlacement();
            }
        }
        else
        {
            if (isCollidingWithGrid && Input.GetMouseButtonDown(0))
            {
                HandlePieceExtraction();
            }
        }

        /*
        if(previousMousePosition != Input.mousePosition)
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
        */
        previousMousePosition = Input.mousePosition;
    }

    //Piece Functions---------------------------------------------
    private void HandlePiecePlacement()
    {
        if (CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, inventory))
        {
            pieceManager.SetPiece(inventory, onGridPosition);
            isHoldingPiece = false;
            Cursor.visible = true;

            HandleTooltip();
        }
        else
        {
            FoodCell selectedCell = CookingHelperFunctions.PieceCompatible(pieceManager.cells, onGridPosition, inventory);
            if (selectedCell == null)
                return;
            pieceManager.SwapPieces(inventory, selectedCell, onGridPosition);
            HandleMouseVisual();
        }
    }
    private void HandlePieceExtraction()
    {
        pieceManager.ExtractPiece(inventory, onGridPosition);
        if (pieceManager.cells.Count > 0)
        {
            isHoldingPiece = true;
            HandleMouseVisual();
            HandleTooltip();
        }
    }
    private void HandlePieceHolder()
    {
        if (isCollidingWithGrid)
        {
            if (CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, inventory))
                pieceHolder.MoveObjectToGrid(onGridPosition, inventory, pieceManager.pieceCenterOffset, inventory.cellScale);
            else
                pieceHolder.MoveObjectAboveGrid(aboveGridPosition, inventory.transform.rotation);
        }
        else
        {
            CalculateOffGridPosition();
            pieceHolder.MoveObjectToPoint(offGridPosition, offGridRotation);
        }
    }
    private bool HandlePieceRotation()
    {
        if (isPlayingAnimation)
            return false;

        if (Input.mouseScrollDelta.y >= 1)
        {
            StartCoroutine(RotatePiece(true));
        }
        else if (Input.mouseScrollDelta.y <= -1)
        {
            StartCoroutine(RotatePiece(false));
        }

        return true;
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

    //Controller support functions--------------------------------
    /*
    private void ResetGridCursor()
    {
        //set the grid to the center of the first grid in list
        if (allGrids.Count != 0)
        {
            onGridPosition.x = (int)allGrids[0].gridSize.x / 2;
            onGridPosition.y = (int)allGrids[0].gridSize.y / 2;
            currentGridManager = allGrids[0];
            gridIndex = 0;
            gridCursor.SetPosition(currentGridManager.gridPositions[onGridPosition.x, onGridPosition.y].transform, true);
            gridCursor.Visible(false);
            gridCursorSet = true;
        }
        else
            gridCursorSet = false;
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

        bool changedGrid = false;

        if (newPos.x < 0)
        {
            finalPos.x = onGridPosition.x;
            if (gridIndex == 0)
            {
                gridIndex = 1;
                currentGridManager = allGrids[1];
                finalPos.x = currentGridManager.gridSize.x - 1;
                changedGrid = true;
            }
        }
        if (newPos.x >= currentGridManager.gridSize.x)
        {
            finalPos.x = onGridPosition.x;
            if (gridIndex == 1)
            {
                gridIndex = 0;
                currentGridManager = allGrids[0];
                finalPos.x = 0;
                changedGrid = true;
            }
        }
        if (newPos.y < 0)
        {
            if (changedGrid)
                finalPos.y = 0;
            else
                finalPos.y = onGridPosition.y;
        }
        if (newPos.y >= currentGridManager.gridSize.y)
        {
            if (changedGrid)
                finalPos.y = currentGridManager.gridSize.y - 1;
            else
                finalPos.y = onGridPosition.y;
        }

        bool moveFast = cursorSpeedupTimer < 0;
        gridCursor.SetPosition(currentGridManager.gridPositions[finalPos.x, finalPos.y], moveFast);
        onGridPosition = finalPos;

        if (gridCursor.visible == false)
            gridCursor.Visible(true);
    }
    */

    //Grid Calculations-------------------------------------------
    private bool CollidingWithGrid()
    {
        Ray ray = inventoryCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, gridLayers))
        {
            if (hit.transform.CompareTag("Grid"))
            {
                Vector3 mouseDirection = (hit.point - inventoryCamera.ScreenToWorldPoint(Input.mousePosition)).normalized;
                aboveGridPosition = hit.point - mouseDirection * aboveGridDistance;
                onGridPosition = CookingHelperFunctions.ConvertPointToGrid(hit.point, hit.transform, pieceManager.pieceCenterOffset, inventory.cellScale);
                worldPosition = hit.point;
                return true;
            }
        }
        return false;
    }
    private void CalculateOffGridPosition()
    {
        Ray ray = inventoryCamera.ScreenPointToRay(Input.mousePosition);
        offGridPosition = ray.origin + ray.direction.normalized * offGridDistance;
        offGridRotation = Quaternion.LookRotation(ray.direction);
    }

    //UI Functions------------------------------------------------
    private void HandleTooltip()
    {
        if (!isCollidingWithGrid || isHoldingPiece)
        {
            ToolTip.instance.OnHoverExit();
            return;
        }

        FoodCell selectedCell = inventory.cells.Find(cell => cell.gridPosition == onGridPosition);
        if (selectedCell == null)
        {
            currentSelectedCell = null;
            ToolTip.instance.OnHoverExit();
            return;
        }

        if (selectedCell != currentSelectedCell)
        {
            currentSelectedCell = selectedCell;

            ToolTipData data = new();
            data.title = selectedCell.cellScore.mainTag.tagName;
            data.description = selectedCell.cellScore.mainTag.tagDescription;
            data.icon = selectedCell.cellScore.mainTag.tagSymbol;
            ToolTip.instance.OnHoverEnter(data, selectedCell.cellVisual.transform.position);
        }
    }
    public void HandleMouseVisual()
    {
        pieceHolder.transform.position = pieceManager.originalCenterPosition;
        Vector3 screenPoint = inventoryCamera.WorldToScreenPoint(pieceManager.originalCenterPosition);
        Mouse.current.WarpCursorPosition(screenPoint);
        Cursor.visible = false;
    }

    //Steam-------------------------------------------------------
    public IEnumerator SetSteamCounterStat(string statName)
    {
        if (SteamManager.Initialized)
        {
            SteamUserStats.GetStat(statName, out float statValue);
            statValue++;
            SteamUserStats.SetStat(statName, statValue);
            SteamUserStats.StoreStats();
        }
        yield return new WaitForSeconds(1);
        StartCoroutine(SetSteamCounterStat(statName));
    }
}
