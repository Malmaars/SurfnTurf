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
    private Trashbin currentTrashbin;

    [Header("Cursor Settings")]
    public bool isKeyboardAndMouse;
    public CookingStationInteractable currentInteractable;
    public bool canMove;
    public float cursorSpeedupTime;
    public float cursorSpeedupTimer;
    public GameObject[] objectsToDisable;
    public TutorialObjectIndex[] objectsForTutorial;
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
    public CinemachineCamera cookingCamera;
    //Player
    private Transform player;
    //UI
    private GridCursor gridCursor;
    private GameObject cookingStationAnimator;
    //Grids
    public List<GridManager> allGrids = new List<GridManager>();
    [HideInInspector] public List<CookwareHolder> allCookware = new List<CookwareHolder>();
    [HideInInspector] public GridManager inventory;

    [Header("Interactions")]
    public Transform spoonStart;
    public Transform spoonEnd;
    public float spoonMaxDistance;
    public bool spoonsLeft;
    private Queue<Spoon> spoonQueue = new Queue<Spoon>();

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
        UIManager.instance.CookingHud.SetActive(true);
        if (!isAnimatingStation)
        {
            StartCoroutine(CookingStationVisual(true));
        }
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, CloseCookingStation);
        base.EnterState();
        BlackBoard.cameraController.SwitchToCamera(cookingCamera, 0.2f);
        //playerAnimator.SetBool("Table", true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameObject.transform.localPosition = player.transform.localPosition;
        gameObject.transform.localRotation = player.GetChild(1).localRotation;
        if (BlackBoard.cookingDatabase.inventoryChanged)
            inventory.LoadIntoGrid(BlackBoard.cookingDatabase.inventoryData);
        //StartCoroutine(SetSteamCounterStat("time_spent_cooking"));
        //speel animatie van cooking station neerzetten af
    }
    public override void ExitState()
    {
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, CloseCookingStation);
        //playerAnimator.SetBool("Table", false);
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
        UIManager.instance.CookingHud.SetActive(false);
        //gameObject.SetActive(false);
    }
    IEnumerator Start()
    {
        yield return new WaitUntil(() => CloudSaveSystem.Instance != null && CloudSaveSystem.Instance.IsInitialized);
        RetrieveReferences();

        foreach (GridManager grid in allGrids)
        {
            grid.ActivateGrid(cellScale);
        }
        foreach (CookwareHolder cookware in allCookware)
        {
            cookware.ShowCookware();
        }
        pieceManager.cellScale = cellScale;

        if (CloudSaveSystem.Instance.data.allGrids.Count == 0)
        {
            CreateGridData();
        }
        else
        {
            LoadGridData();
        }
        for (int i = 0; i < 3; i++)
        {
            AddSpoon(i);
            AddSpoon(i);
            AddSpoon(i);
        }
    }
    private void LoadGridData()
    {
        foreach (var gridData in CloudSaveSystem.Instance.data.allGrids)
        {

            GridManager matchingGrid = allGrids.Find(g => g.gridName == gridData.gridName);
            if (matchingGrid != null)
            {
                matchingGrid.LoadIntoGrid(gridData);
            }
        }
        //inventory.LoadIntoGrid(BlackBoard.cookingDatabase.inventoryData);
        //BlackBoard.cookingDatabase.SaveInventory(inventory.cells);
    }
    public void CreateGridData()
    {
        if (BlackBoard.cookingDatabase.inventoryChanged)
        {
            inventory.LoadIntoGrid(BlackBoard.cookingDatabase.inventoryData);
            BlackBoard.cookingDatabase.SaveInventory(inventory.cells);
            BlackBoard.cookingDatabase.inventoryChanged = false;
        }
        foreach (GridManager gridManager in allGrids)
        {
            GridData gridData = new GridData(gridManager.gridName);
            GridData matchingGrid = CloudSaveSystem.Instance.data.allGrids.Find(g => g.gridName == gridData.gridName);
            foreach (FoodCell cell in gridManager.cells)
            {
                List<Vector2Int> groupCells = new List<Vector2Int>();
                foreach (FoodCell groupCell in cell.groupCells)
                {
                    groupCells.Add(groupCell.gridPosition);
                }
                gridData.foodCells.Add(new FoodCellData(cell.gridPosition.x, cell.gridPosition.y, cell.cellID, groupCells, cell.cellTexturePosition, cell.textureGridSize, cell.originalIngredient));
            }
            if (matchingGrid != null)
            {
                CloudSaveSystem.Instance.data.allGrids.Remove(matchingGrid);
                CloudSaveSystem.Instance.data.allGrids.Add(gridData);
            }
            else
            {
                CloudSaveSystem.Instance.data.allGrids.Add(gridData);
            }

        }
    }
    private void RetrieveReferences()
    {
        pieceHolder = transform.GetChild(0).GetComponent<PieceHolder>();
        pieceManager = transform.GetChild(0).GetComponent<PieceManager>();
        pieceAnimationHelper = transform.GetChild(0).GetChild(0);

        gridCursor = transform.GetChild(1).GetComponent<GridCursor>();

        player = BlackBoard.playerBody;
        cookingStationAnimator = transform.GetChild(3).gameObject;
        cookingStationAnimator.SetActive(false);

        allGrids.AddRange(transform.GetComponentsInChildren<GridManager>());
        allCookware.AddRange(transform.GetComponentsInChildren<CookwareHolder>());
        inventory = GetComponentInChildren<Inventory>();

        InputSystem.onActionChange += InputActionChangeCallback;
    }
    private void hideObjects(bool hide)
    {
        foreach (GameObject obj in objectsToDisable)
        {
            obj.SetActive(hide);
        }
    }

    private void InputActionChangeCallback(object obj, InputActionChange change)
    {
        if (change == InputActionChange.ActionPerformed)
        {
            InputAction receivedInputAction = (InputAction)obj;
            InputDevice lastDevice = receivedInputAction.activeControl.device;

            isKeyboardAndMouse = lastDevice.name.Equals("Keyboard") || lastDevice.name.Equals("Mouse");
        }
    }

    //Update------------------------------------------------------
    private void Update()
    {
        if (isKeyboardAndMouse)
        {
            if (gridCursor.visible == true)
                gridCursor.Visible(false);

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
                if (isCollidingWithGrid)
                    currentGridManager.OnHover(onGridPosition);
                if (Input.GetMouseButtonDown(0))
                {
                    HandlePieceExtraction();
                    if (isCollidingWithGrid)
                        currentGridManager.OnAction();
                }

                HandlePhysicalButton();

                if (Input.GetMouseButtonUp(0) && isExtractingWhole)
                {
                    isExtractingWhole = false;
                    currentExtractingTime = 0;
                }
            }
            previousMousePosition = Input.mousePosition;
        }
        else
        {
            HandleControllerInput();
            HandleControllerMovement();
            HandleTooltip();
            if (isHoldingPiece)
                HandleControllerPieceHolder();
            else
            {
                if (isCollidingWithGrid)
                    currentGridManager.OnHover(onGridPosition);
            }
        }
        Cursor.visible = true;
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
            hideObjects(true);
            isAnimatingStation = false;
        }
        else
        {
            HideGrids();
            hideObjects(false);
            cookingStationAnimator.GetComponent<Animator>().SetBool("isOpen", false);

            playerAnimator.SetBool("CookingStation", false);

            yield return new WaitForSeconds(1.5f);

            cookingStationAnimator.SetActive(false);

            isAnimatingStation = false;

            nextState = typeof(MovementController);
        }
    }

    public override void PauseGame(InputAction.CallbackContext context)
    {
        if (!isAnimatingStation)
            base.PauseGame(context);
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
                if (isKeyboardAndMouse)
                    Cursor.visible = true;

                HandleTooltip();
                CreateGridData();
            }
            else
            {
                if (!currentGridManager.mayExtract || currentGridManager.cells.Count == 0)
                    return;
                FoodCell selectedCell = CookingHelperFunctions.PieceCompatible(pieceManager.cells, onGridPosition, currentGridManager);
                if (selectedCell == null)
                    return;
                pieceManager.SwapPieces(currentGridManager, selectedCell, onGridPosition);
                HandleMouseVisual();
                CreateGridData();
            }
        }
        else
        {
            if (currentPlate != null)
            {
                pieceManager.SetPlate(currentPlate);
                if (pieceManager.cells.Count == 0)
                {
                    isHoldingPiece = false;
                    Cursor.visible = true;
                    CreateGridData();
                }
            }
            else if (currentTrashbin != null)
            {
                pieceManager.RemoveDish();
                isHoldingPiece = false;
                Cursor.visible = true;
                CreateGridData();
            }
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
                pieceManager.ExtractPiece(currentGridManager, onGridPosition);
                if (pieceManager.cells.Count > 0)
                {
                    isHoldingPiece = true;
                    pieceHolder.transform.position = pieceManager.originalCenterPosition;
                    HandleMouseVisual();
                    CreateGridData();
                }
                ToolTip.instance.OnHoverExit();
            }
            else
            {
                pieceManager.ExtractPiece(currentGridManager, onGridPosition);
                if (pieceManager.cells.Count > 0)
                {
                    isHoldingPiece = true;
                    HandleMouseVisual();
                    CreateGridData();
                    HandleTooltip();
                }
            }
        }
        else
        {
            if (currentPlate == null)
                return;
            if (currentPlate.mainCells.Count == 0)
                return;
            pieceManager.ExtractPlate(currentPlate);
            isHoldingPiece = true;
            HandleMouseVisual();
            CreateGridData();
        }
    }
    public void ForcePieceExtraction(GridManager selectedGrid)
    {
        if (isHoldingPiece || selectedGrid.cells.Count == 0)
            return;
        pieceManager.ExtractPiece(selectedGrid, selectedGrid.cells[0].gridPosition);
        if (pieceManager.cells.Count > 0)
        {
            isHoldingPiece = true;
            pieceHolder.transform.position = pieceManager.originalCenterPosition;
            HandleMouseVisual();
        }
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

        if (isKeyboardAndMouse)
        {
            if (Input.mouseScrollDelta.y >= 1)
            {
                StartCoroutine(RotatePiece(true));
            }
            else if (Input.mouseScrollDelta.y <= -1)
            {
                StartCoroutine(RotatePiece(false));
            }
            else if (Input.GetMouseButtonDown(1))
            {
                StartCoroutine(RotatePiece(true));
            }
        }
        else
        {
            StartCoroutine(RotatePiece(true));
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
            if (i == index)
            {
                allCookware[i].ShowCookware();
            }
            else
            {
                allCookware[i].HideCookware();
            }
        }
    }
    public void AddSpoon(int spoonID)
    {
        Spoon newSpoon = Instantiate(BlackBoard.cookingDatabase.GetSpoon(spoonID), spoonStart).GetComponent<Spoon>();
        spoonQueue.Enqueue(newSpoon);
        PrintSpoons();
        OrderSpoons();
        Debug.Log($"Added {newSpoon.name} spoon with {newSpoon.durability} durability.");
    }
    public void UseSpoon()
    {
        if (spoonQueue.Count == 0)
        {
            Debug.LogWarning("No spoons available!");
            return;
        }

        Spoon currentSpoon = spoonQueue.Peek();

        bool isBroken = currentSpoon.Use();
        Debug.Log($"Used {currentSpoon.name} spoon. Remaining durability: {currentSpoon.durability}");

        if (isBroken)
        {
            Debug.Log($"{currentSpoon.name} spoon broke!");
            spoonQueue.Dequeue(); // Remove broken spoon
            Destroy(currentSpoon.gameObject);
            OrderSpoons();
        }
        PrintSpoons();
    }
    public void PrintSpoons()
    {
        Debug.Log("Current spooncount in queue:");
        int totalDurability = 0;
        foreach (Spoon spoon in spoonQueue)
        {
            totalDurability += spoon.durability;
        }
        Debug.Log($"{spoonQueue.Count} spoons, with a total Durability of: {totalDurability}");
        spoonsLeft = totalDurability > 0;
    }

    public void OrderSpoons()
    {
        if (spoonQueue.Count == 0)
            return;
        int spoonCount = spoonQueue.Count;
        float totalDistance = Vector3.Distance(spoonStart.localPosition, spoonEnd.localPosition);

        float stepDistance = spoonMaxDistance;

        float maxNeededDistance = (spoonCount - 1) * spoonMaxDistance;
        if (maxNeededDistance > totalDistance)
        {
            stepDistance = totalDistance / (spoonCount - 1);
        }

        Vector3 direction = (spoonEnd.localPosition - spoonStart.localPosition).normalized;

        int i = 0;
        foreach (Spoon spoon in spoonQueue)
        {
            Vector3 newPosition = direction * stepDistance * i;
            spoon.SetPosition(newPosition);
            i++;
        }
    }

    //Controller support functions--------------------------------
    private void HandleControllerMovement()
    {
        Vector2Int playerInput = Vector2Int.RoundToInt(InputDistributor.playerInputActions.Cooking.DirectionalInput.ReadValue<Vector2>());

        if (playerInput == Vector2Int.zero)
        {
            cursorSpeedupTimer = cursorSpeedupTime;
            canMove = true;
            return;
        }

        if (currentInteractable.interactionType == CookingStationInteractable.CookingInteractableType.Grid)
        {
            MoveGridCursor(playerInput);
        }
        else
        {
            if (canMove)
            {
                bool switched = SwitchInteractable(playerInput);
                if (switched)
                    canMove = false;
            }
        }
    }

    public bool SwitchInteractable(Vector2Int playerInput)
    {
        bool switched = false;

        if (playerInput.x < 0 && playerInput.y == 0)
        {
            if (currentInteractable.left != null)
            {
                currentInteractable = currentInteractable.left;
                switched = true;
            }
        }
        else if (playerInput.x > 0 && playerInput.y == 0)
        {
            if (currentInteractable.right != null)
            {
                currentInteractable = currentInteractable.right;
                switched = true;
            }
        }
        else if (playerInput.y < 0 && playerInput.x == 0)
        {
            if (currentInteractable.down != null)
            {
                currentInteractable = currentInteractable.down;
                switched = true;
            }
        }
        else if (playerInput.y > 0 && playerInput.x == 0)
        {
            if (currentInteractable.up != null)
            {
                currentInteractable = currentInteractable.up;
                switched = true;
            }
        }

        if (switched)
        {
            isCollidingWithGrid = false;
            if (currentPhysicalButton != null)
                currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseExit.Invoke();
            currentPhysicalButton = null;
            currentPlate = null;
            currentTrashbin = null;
            if (gridCursor.visible == true)
                gridCursor.Visible(false);

            switch (currentInteractable.interactionType)
            {
                case CookingStationInteractable.CookingInteractableType.Grid:
                    currentGridManager = currentInteractable.transform.GetComponent<GridManager>();

                    if (playerInput.x < 0)
                        onGridPosition = new Vector2Int(currentGridManager.gridSize.x - 1, currentGridManager.gridSize.y / 2);
                    else if (playerInput.x > 0)
                        onGridPosition = new Vector2Int(0, currentGridManager.gridSize.y / 2);
                    else if (playerInput.y < 0)
                        onGridPosition = new Vector2Int(currentGridManager.gridSize.x / 2, currentGridManager.gridSize.y - 1);
                    else if (playerInput.y > 0)
                        onGridPosition = new Vector2Int(currentGridManager.gridSize.x / 2, 0);

                    bool moveFast = cursorSpeedupTimer < 0;
                    gridCursor.SetPosition(currentGridManager.gridPositions[onGridPosition.x, onGridPosition.y], moveFast);

                    if (gridCursor.visible == false && !isHoldingPiece)
                        gridCursor.Visible(true);

                    isCollidingWithGrid = true;

                    break;
                case CookingStationInteractable.CookingInteractableType.Button:
                    currentPhysicalButton = currentInteractable.transform.gameObject;
                    currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseEnter.Invoke();
                    break;
                case CookingStationInteractable.CookingInteractableType.Plate:
                    currentPlate = currentInteractable.transform.GetComponent<PlateHolder>();
                    break;
                case CookingStationInteractable.CookingInteractableType.Trash:
                    currentTrashbin = currentInteractable.transform.GetComponent<Trashbin>();
                    break;
                case CookingStationInteractable.CookingInteractableType.Book:
                    break;
                default:
                    break;
            }
            return true;
        }
        else
            return false;
    }

    private void MoveGridCursor(Vector2Int playerInput)
    {
        cursorSpeedupTimer -= Time.deltaTime;

        if (!gridCursor.canMove)
            return;

        Vector2Int newPos = onGridPosition + playerInput;
        Vector2Int finalPos = newPos;

        bool switched = false;

        if (newPos.x < 0)
        {
            switched = SwitchInteractable(playerInput);
            if (!switched)
                finalPos.x = onGridPosition.x;
            else
                finalPos.x = currentGridManager.gridSize.x - 1;
        }
        else if (newPos.x >= currentGridManager.gridSize.x)
        {
            switched = SwitchInteractable(playerInput);
            if (!switched)
                finalPos.x = onGridPosition.x;
            else
                finalPos.x = 0;
        }

        if (!switched)
        {
            if (newPos.y < 0)
            {
                switched = SwitchInteractable(playerInput);
                if (!switched)
                    finalPos.y = onGridPosition.y;
                else
                    finalPos.y = currentGridManager.gridSize.y - 1;
            }
            if (newPos.y >= currentGridManager.gridSize.y)
            {
                switched = SwitchInteractable(playerInput);
                if (!switched)
                    finalPos.y = onGridPosition.y;
                else
                    finalPos.y = 0;
            }
        }

        if (!switched)
        {
            bool moveFast = cursorSpeedupTimer < 0;
            gridCursor.SetPosition(currentGridManager.gridPositions[finalPos.x, finalPos.y], moveFast);
            onGridPosition = finalPos;

            if (gridCursor.visible == false && !isHoldingPiece)
                gridCursor.Visible(true);
        }
    }

    public void HandleControllerInput()
    {
        if (InputDistributor.playerInputActions.Cooking.Primary.WasPerformedThisFrame())
            HandlePrimaryInput();
        if (InputDistributor.playerInputActions.Cooking.Primary.WasReleasedThisFrame())
            HandlePrimaryCancel();
        if (InputDistributor.playerInputActions.Cooking.Secondary.WasPerformedThisFrame())
            HandleSecondaryInput();
        if (InputDistributor.playerInputActions.Cooking.Secondary.WasReleasedThisFrame())
            HandleSecondaryCancel();
    }

    public void HandlePrimaryInput()
    {
        if (isHoldingPiece)
        {
            if (!isPlayingAnimation)
            {
                HandlePiecePlacement();
            }
        }
        else
        {
            HandlePieceExtraction();
            if (isCollidingWithGrid)
                currentGridManager.OnAction();

            if (currentPhysicalButton != null)
                currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseDown.Invoke();
        }

        if (isHoldingPiece)
        {
            if (gridCursor.visible == true)
                gridCursor.Visible(false);
        }
        else
        {
            if (gridCursor.visible == false)
                gridCursor.Visible(true);
        }
    }

    public void HandlePrimaryCancel()
    {
        if (currentPhysicalButton != null)
            currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseUp.Invoke();
    }

    public void HandleSecondaryInput()
    {
        if (isHoldingPiece)
        {
            HandlePieceRotation();
        }
    }

    public void HandleSecondaryCancel()
    {

    }

    private void HandleControllerPieceHolder()
    {
        if (isCollidingWithGrid)
        {
            if (CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, currentGridManager))
                pieceHolder.MoveObjectToGrid(onGridPosition, currentGridManager, pieceManager.pieceCenterOffset, cellScale);
            else
            {
                if (onGridPosition.x < 0 || onGridPosition.x > currentGridManager.gridPositions.GetLength(0) || onGridPosition.y < 0 || onGridPosition.y > currentGridManager.gridPositions.GetLength(1))
                    return;
                Vector3 lookDirection = (currentGridManager.gridPositions[onGridPosition.x, onGridPosition.y].position - Camera.main.transform.position).normalized;
                Vector3 newPosition = currentGridManager.gridPositions[onGridPosition.x, onGridPosition.y].position - lookDirection * aboveGridDistance;
                pieceHolder.MoveObjectAboveGrid(newPosition, currentGridManager.transform.rotation);
            }
        }
        else
        {
            Vector3 lookDirection = (currentInteractable.transform.position - Camera.main.transform.position).normalized;
            Ray ray = new Ray(Camera.main.transform.position, lookDirection);
            Vector3 newPosition = ray.origin + ray.direction.normalized * offGridDistance;
            Quaternion newRotation = Quaternion.LookRotation(ray.direction);
            pieceHolder.MoveObjectToPoint(newPosition, newRotation);
        }

    }

    //Grid Calculations-------------------------------------------
    private bool CollidingWithGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, gridLayers))
        {
            currentPlate = null;
            currentTrashbin = null;
            if (hit.transform.tag == "Grid")
            {
                currentGridManager = hit.transform.GetComponent<GridManager>();
                Vector3 mouseDirection = (hit.point - Camera.main.ScreenToWorldPoint(Input.mousePosition)).normalized;
                aboveGridPosition = hit.point - mouseDirection * aboveGridDistance;
                onGridPosition = CookingHelperFunctions.ConvertPointToGrid(hit.point, hit.transform, pieceManager.pieceCenterOffset, cellScale);
                worldPosition = hit.point;
                return true;
            }
            if (hit.transform.tag == "Plate")
            {
                currentPlate = hit.transform.GetComponent<PlateHolder>();
            }
            if (hit.transform.tag == "Trashbin")
            {
                currentTrashbin = hit.transform.GetComponent<Trashbin>();
            }
        }
        if (currentGridManager != null && currentGridManager.hovering)
            currentGridManager.OnExit();
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
                if (currentPhysicalButton != hit.transform.gameObject)
                {
                    currentPhysicalButton = hit.transform.gameObject;
                    currentPhysicalButton.GetComponent<PhysicalButton>().OnMouseEnter.Invoke();
                }
                return true;
            }
            else
            {
                if (currentPhysicalButton != null)
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
    public void SetCurrentGrid(GridManager selectedGrid)
    {
        currentGridManager = selectedGrid;
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
