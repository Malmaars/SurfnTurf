using System;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using Steamworks;

public class CookingManager : PlayerState
{
    [Header("Piece Holder Settings")]
    [SerializeField] private float rotationDuration = 0.25f;
    [SerializeField] private float offGridDistance = 10;
    [SerializeField] private float aboveGridDistance = 1;
    [SerializeField] private float timeToExtractWhole;
    //PieceHolder stats
    [NaughtyAttributes.ReadOnly] public bool isHoldingPiece;
    [NaughtyAttributes.ReadOnly] public bool isPlayingAnimation;
    [NaughtyAttributes.ReadOnly] public bool isExtractingWhole = false;
    private float currentExtractingTime = 0;

    [Header("Grid Settings")]
    [SerializeField] private float cellScale;
    [SerializeField] private LayerMask gridLayers;
    //Grid Stats
    [NaughtyAttributes.ReadOnly] public bool isCollidingWithGrid;
    private int cookwareIndex;
    private int gridIndex;
    private GridManager currentGridManager;
    private FoodCell currentSelectedCell;
    private PlateHolder currentPlate;
    private GameObject currentPhysicalButton;

    [Header("Cursor Settings")]
    public float cursorSpeedupTime;
    public float cursorSpeedupTimer;
    //Cursor Stats
    private bool gridCursorSet;

    [Header("References")]
    public Animator playerAnimator;

    //References
    //PieceHolder
    private PieceHolder pieceHolder;
    private PieceManager pieceManager;
    private Transform pieceAnimationHelper;
    //Camera
    private CookingCameraController cameraController;
    //Player
    private Transform player;
    //UI
    private GridCursor gridCursor;
    private GameObject cookingStationAnimator;
    //Grids
    [HideInInspector] public List<GridManager> allGrids = new List<GridManager>();
    [HideInInspector] public List<CookwareHolder> allCookware = new List<CookwareHolder>();
    [HideInInspector] public GridManager inventory;

    //animations
    private bool isAnimatingStation;

    //Mouse position on and off grid
    private Vector3 previousMousePosition;
    private Vector3 worldPosition;
    private Vector3 aboveGridPosition;
    private Vector2Int onGridPosition;
    private Vector3 offGridPosition;
    private quaternion offGridRotation;
    
    //Initialization & Exiting Cooking State----------------------
    public override void InitStateTransitions()
    {
        base.InitStateTransitions();

        transitions.Add(new PlayerStateTransition(typeof(MovementController), () => nextState == typeof(MovementController)));
        transitions.Add(new PlayerStateTransition(typeof(PauseState), () => nextState == typeof(PauseState)));
    }
    public override void EnterState()
    {
        gameObject.SetActive(true);
        if (!isAnimatingStation)
        {
            StartCoroutine(CookingStationVisual(true));
        }
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, CloseCookingStation);
        base.EnterState();
        cameraController.EnterState();
        cameraController.SetCamera(1);
        //playerAnimator.SetBool("Table", true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameObject.transform.localPosition = player.transform.localPosition;
        gameObject.transform.localRotation = player.GetChild(1).localRotation;
        ResetGridCursor();
        if(BlackBoard.cookingDatabase.inventoryChanged)
            inventory.LoadIntoGrid(BlackBoard.cookingDatabase.inventoryData);
        //StartCoroutine(SetSteamCounterStat("time_spent_cooking"));
        //speel animatie van cooking station neerzetten af
    }
    public override void ExitState()
    {
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, CloseCookingStation);
        //playerAnimator.SetBool("Table", false);
        if(cameraController != null)
            cameraController.ExitState();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        base.ExitState();
        if (inventory != null)
            BlackBoard.cookingDatabase.SaveInventory(inventory.cells);
        foreach (GridManager grid in allGrids)
        {
            if (!grid.alwaysOn)
            {
                grid.TurnOff();
            }
        }
        //StopCoroutine(SetSteamCounterStat("time_spent_cooking"));
        ToolTip.instance.OnHoverExit();
        gameObject.SetActive(false);
    }
    private void Awake()
    {
        RetrieveReferences();
        foreach (CookwareHolder cookware in allCookware)
        {
            cookware.UnlockCookware(cellScale);
            cookware.HideCookware();
        }
        if (allCookware.Count == 1)
        {
            allCookware[0].ShowCookware();
        }
        inventory.ActivateGrid(cellScale);
        pieceManager.cellScale = cellScale;
    }
    private void RetrieveReferences()
    {
        pieceHolder = transform.GetChild(0).GetComponent<PieceHolder>();
        pieceManager = transform.GetChild(0).GetComponent<PieceManager>();
        pieceAnimationHelper = transform.GetChild(0).GetChild(0);

        gridCursor = transform.GetChild(1).GetComponent<GridCursor>();

        cameraController = transform.GetChild(2).GetComponent<CookingCameraController>();

        player = FindAnyObjectByType<MovementController>().transform;
        cookingStationAnimator = transform.GetChild(3).gameObject;
        cookingStationAnimator.SetActive(false);

        allGrids.AddRange(transform.GetComponentsInChildren<GridManager>());
        allCookware.AddRange(transform.GetComponentsInChildren<CookwareHolder>());
        inventory = GetComponentInChildren<Inventory>();
    }

    //Update------------------------------------------------------
    private void Update()
    {
        isCollidingWithGrid = CollidingWithGrid();
        HandleCookwareSwitching();
        HandleTooltip();

        if (isHoldingPiece)
        {
            HandlePieceHolder();

            if (HandlePieceRotation() && Input.GetMouseButtonDown(0))
            {
                HandlePiecePlacement();
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                HandlePieceExtraction();
            }

            HandlePhysicalButton();

            if (Input.GetMouseButtonUp(0) && isExtractingWhole)
            {
                isExtractingWhole = false;
                currentExtractingTime = 0;
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

    //CookingStation Functions------------------------------------
    public void CloseCookingStation(InputAction.CallbackContext context)
    {
        if (!isAnimatingStation)
        {
            StartCoroutine(CookingStationVisual(false));
        }
    }

    IEnumerator CookingStationVisual(bool open)
    {
        isAnimatingStation = true;
        if (open)
        {
            HideGrids();
            cookingStationAnimator.SetActive(true);
            cookingStationAnimator.GetComponent<Animator>().SetBool("isOpen", true);

            playerAnimator.SetTrigger("CookingStationOpen");
            playerAnimator.SetBool("CookingStation", true);

            yield return new WaitForSeconds(1.5f);

            ShowGrids();
            isAnimatingStation = false;
        }
        else
        {
            HideGrids();
            cookingStationAnimator.GetComponent<Animator>().SetBool("isOpen", false);

            playerAnimator.SetBool("CookingStation", false);

            yield return new WaitForSeconds(1.5f);

            cookingStationAnimator.SetActive(false);

            isAnimatingStation = false;

            nextState = typeof(MovementController);
        }
    }
    public void ShowGrids()
    {
        foreach (GridManager grid in allGrids)
        {
            grid.ShowGrid();
        }
    }
    public void HideGrids()
    {
        foreach (GridManager grid in allGrids)
        {
            grid.HideGrid();
        }
    }

    //Piece Functions---------------------------------------------
    private void HandlePiecePlacement()
    {
        if (isCollidingWithGrid)
        {
            if (CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, currentGridManager))
            {
                pieceManager.SetPiece(currentGridManager, onGridPosition);
                isHoldingPiece = false;
                Cursor.visible = true;

                HandleTooltip();
            }
            else
            {
                if (!currentGridManager.mayExtract)
                    return;
                FoodCell selectedCell = CookingHelperFunctions.PieceCompatible(pieceManager.cells, onGridPosition, currentGridManager);
                if (selectedCell == null)
                    return;
                pieceManager.SwapPieces(currentGridManager, selectedCell, onGridPosition);
                HandleMouseVisual();
            }
        }
        else
        {
            if (currentPlate == null)
                return;
            pieceManager.SetPlate(currentPlate);
            isHoldingPiece = false;
            Cursor.visible = true;
        }
    }
    private void HandlePieceExtraction()
    {
        if (isCollidingWithGrid)
        {
            if (!currentGridManager.mayExtract)
                return;
            if (currentGridManager.extractWhole)
            {
                isExtractingWhole = true;
                StartCoroutine(ExtractWhole());
                ToolTip.instance.OnHoverExit();
            }
            else
            {
                pieceManager.ExtractPiece(currentGridManager, onGridPosition);
                if (pieceManager.cells.Count > 0)
                {
                    isHoldingPiece = true;
                    HandleMouseVisual();
                    HandleTooltip();
                }
            }
        }
        else
        {
            if (currentPlate == null || currentPlate.mainCells.Count == 0)
                return;
            pieceManager.ExtractPlate(currentPlate);
            isHoldingPiece = true;
            HandleMouseVisual();
        }
    }
    IEnumerator ExtractWhole()
    {
        while (currentExtractingTime < timeToExtractWhole)
        {
            if (isExtractingWhole)
            {
                currentExtractingTime += Time.deltaTime;
            }

            yield return new WaitForEndOfFrame();
        }

        if (isExtractingWhole)
        {
            pieceManager.ExtractPiece(currentGridManager, onGridPosition);
            if (pieceManager.cells.Count > 0)
            {
                isHoldingPiece = true;
                pieceHolder.transform.position = pieceManager.originalCenterPosition;
                HandleMouseVisual();
            }
            isExtractingWhole = false;
        }

        yield return null;
    }
    private void HandlePieceHolder()
    {
        if (isCollidingWithGrid)
        {
            if (CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, currentGridManager))
                pieceHolder.MoveObjectToGrid(onGridPosition, currentGridManager, pieceManager.pieceCenterOffset, cellScale);
            else
                pieceHolder.MoveObjectAboveGrid(aboveGridPosition, currentGridManager.transform.rotation);
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

    //Cookware functions------------------------------------------
    public void HandleCookwareSwitching()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SwitchCookware(-1);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            SwitchCookware(1);
        }
    }
    void SwitchCookware(int direction)
    {
        if (allCookware.Count <= 1) return;

        int startIndex = cookwareIndex;
        int index = cookwareIndex;

        do
        {
            index = (index + direction + allCookware.Count) % allCookware.Count;

            if (allCookware[index].unlocked) // assuming IsUnlocked is a bool on your CookwareHolder
            {
                if (index != cookwareIndex)
                {
                    // Do the switch
                    cookwareIndex = index;
                    ActivateCookware(index);
                }
                return;
            }

        } while (index != startIndex);
    }
    void ActivateCookware(int index)
    {
        for (int i = 0; i < allCookware.Count; i++)
        {
            if(i == index)
            {
                allCookware[i].ShowCookware();
            }
            else
            {
                allCookware[i].HideCookware();
            }
        }
    }

    //Controller support functions--------------------------------
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
            if(gridIndex == 0)
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
                finalPos.y = currentGridManager.gridSize.y-1;
            else
                finalPos.y = onGridPosition.y;
        }

        bool moveFast = cursorSpeedupTimer < 0;
        gridCursor.SetPosition(currentGridManager.gridPositions[finalPos.x, finalPos.y], moveFast);
        onGridPosition = finalPos;

        if (gridCursor.visible == false)
            gridCursor.Visible(true);
    }

    //Grid Calculations-------------------------------------------
    private bool CollidingWithGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, gridLayers))
        {
            currentPlate = null;
            if (hit.transform.tag == "Grid")
            {
                currentGridManager = hit.transform.GetComponent<GridManager>();
                Vector3 mouseDirection = (hit.point - Camera.main.ScreenToWorldPoint(Input.mousePosition)).normalized; 
                aboveGridPosition = hit.point - mouseDirection * aboveGridDistance;
                onGridPosition = CookingHelperFunctions.ConvertPointToGrid(hit.point, hit.transform, pieceManager.pieceCenterOffset, cellScale);
                worldPosition = hit.point;
                return true;
            }
            if(hit.transform.tag == "Plate")
            {
                currentPlate = hit.transform.GetComponent<PlateHolder>();
            }
        }
        return false;
    }
    private void CalculateOffGridPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        offGridPosition = ray.origin + ray.direction.normalized * offGridDistance;
        offGridRotation = Quaternion.LookRotation(ray.direction);
    }

    //UI Functions------------------------------------------------
    private bool CollidingWithPhysicalButton()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, gridLayers))
        {
            if (hit.transform.tag == "PhysicalButton")
            {
                
                if(currentPhysicalButton != hit.transform.gameObject)
                {
                    currentPhysicalButton = hit.transform.gameObject;
                    currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseEnter.Invoke();
                }
                return true;
            }
            else
            {
                if(currentPhysicalButton != null)
                {
                    currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseExit.Invoke();
                    currentPhysicalButton = null;
                }
            }
        }
        return false;
    }
    private void HandlePhysicalButton()
    {
        if (CollidingWithPhysicalButton())
        {
            if (Input.GetMouseButtonDown(0))
            {
                currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseDown.Invoke();
            }
            else if (Input.GetMouseButtonUp(0))
            {
                currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseUp.Invoke();
            }
        }
    }
    private void HandleTooltip()
    {
        if (!isCollidingWithGrid || isHoldingPiece)
        {
            ToolTip.instance.OnHoverExit();
            return;
        }

        FoodCell selectedCell = currentGridManager.cells.Find(cell => cell.gridPosition == onGridPosition);
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
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(pieceManager.originalCenterPosition);
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
