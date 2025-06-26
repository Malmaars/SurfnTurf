using System;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Unity.Mathematics;
using System.Collections;
using Unity.Cinemachine;
using Steamworks;
using NaughtyAttributes;

public class PickUpUI : MonoBehaviour
{
    public Canvas canvas;
    public UIOption LeftOption;
    public UIOption RightOption;
    public bool uIActive = false;
    public EventReference feulSound;
    public EventReference ingrediantSound;
    public EventReference toolSound;

    private float shapePreviewSize = 100f;
    private float pieceOffset = 0.1f;

    [Header("Piece Holder Settings")]
    [SerializeField] private float rotationDuration = 0.25f;
    [SerializeField] private float offGridDistance = 10;
    [SerializeField] private float aboveGridDistance = 1;
    [SerializeField] private float timeToExtractWhole;
    [SerializeField] private float cellScale;
    //PieceHolder stats
    [NaughtyAttributes.ReadOnly] public bool isHoldingPiece;
    [NaughtyAttributes.ReadOnly] public bool isPlayingAnimation;
    [NaughtyAttributes.ReadOnly] public bool isExtractingWhole = false;
    //private float currentExtractingTime = 0;

    [Header("Audio Refs")]
    public EventReference placePiece;
    public EventReference pickUpPiece;

    [Header("Grid Settings")]
    [SerializeField] private LayerMask gridLayers;
    //Grid Stats
    [NaughtyAttributes.ReadOnly] public bool isCollidingWithGrid;
    private GridManager currentGridManager;
    private FoodCell currentSelectedCell;

    [Header("Cursor Settings")]
    public bool isKeyboardAndMouse;
    public CookingStationInteractable currentInteractable;
    public bool canMove;
    public float cursorSpeedupTime;
    public float cursorSpeedupTimer;
    //Cursor Stats
    private bool gridCursorSet;

    [Header("References")]
    //Cam
    public Camera uiCam;
    //PieceHolder
    public PieceHolder pieceHolder;
    public PieceManager pieceManager;
    public Transform pieceAnimationHelper;
    //UI
    public GridCursor gridCursor;
    public GridManager inventory;
    //Mouse position on and off grid
    private Vector3 previousMousePosition;
    private Vector3 worldPosition;
    private Vector3 aboveGridPosition;
    private Vector2Int onGridPosition;
    private Vector3 offGridPosition;
    private Quaternion offGridRotation;

	PickUpIngredient currentIngredient;


	private void Start()
    {
        canvas.enabled = false;

        inventory.ActivateGrid(0);
        inventory.gameObject.SetActive(false);

        InputSystem.onActionChange += InputActionChangeCallback;
        LeftOption.fuelIcon.SetActive(false);
        LeftOption.toolIcon.SetActive(false);
        RightOption.fuelIcon.SetActive(false);
        RightOption.toolIcon.SetActive(false);
    }
    public void ShowUI(PickUpOptionData leftOptionData, PickUpOptionData rightOptionData, PickUpIngredient _ingredient)
    {
        currentIngredient = _ingredient;
        uIActive = true;
        //Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UIManager.instance.SetVisibleUI(false);
        if(leftOptionData.ingredientData != null)
            LeftOption.title.text = leftOptionData.ingredientData.ingredientName;
        else
            LeftOption.title.text = leftOptionData.title;
        LeftOption.description.text = leftOptionData.description;
        //LeftOption.icon.gameObject.SetActive(leftOptionData.showIcon);
        //LeftOption.icon.sprite = leftOptionData;
        //LeftOption.previewContainer.SetActive(leftOptionData.showPreview);
        /*
        if (leftOptionData.showPreview)
        {
            GeneratePreview(LeftOption.previewContainer, leftOptionData.ingredientData);
        }
        */
        if(rightOptionData.ingredientData != null)
            RightOption.title.text = rightOptionData.ingredientData.ingredientName;
        else
            RightOption.title.text = rightOptionData.title;
        RightOption.description.text = rightOptionData.description;
        //RightOption.icon.gameObject.SetActive(rightOptionData.showIcon);
        //RightOption.icon.sprite = rightOptionData.icon;
        //RightOption.previewContainer.SetActive(rightOptionData.showPreview);
        /*
        if (rightOptionData.showPreview)
        {
            GeneratePreview(RightOption.previewContainer, rightOptionData.ingredientData);
        }
        */
        SetupUIOption(LeftOption, leftOptionData);
        SetupUIOption(RightOption, rightOptionData);

        EventSystem.current.SetSelectedGameObject(LeftOption.button.gameObject);
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Interactions.ExitPickUp, ExitPickUpUIOnInput);


		canvas.enabled = true;
        OpenPickupScreen();
    }
    private void SetupUIOption(UIOption option, PickUpOptionData optionData)
    {
        option.title.text = optionData.title;
        option.description.text = optionData.description;

        switch (optionData.pickUpType)
        {
            case PickUpType.Ingredient:
                option.fuelIcon.SetActive(false);
                option.toolIcon.SetActive(false);
                option.previewContainer.SetActive(true);
                GeneratePreview(option.previewContainer, optionData.ingredientData);
                break;
            case PickUpType.Fuel:
                option.fuelIcon.SetActive(true);
                option.toolIcon.SetActive(false);
                option.previewContainer.SetActive(false);
                break;
            case PickUpType.Tool:
                option.fuelIcon.SetActive(false);
                option.toolIcon.SetActive(true);
                option.previewContainer.SetActive(false);
                break;
        }

        option.button.onClick.AddListener(() => OnOptionSelected(optionData));
        option.button.interactable = true;
    }

    private void OnOptionSelected(PickUpOptionData selectedOption)
    {
        switch (selectedOption.pickUpType)
        {
            case PickUpType.Ingredient:
				BlackBoard.cookingDatabase.SaveInventory(inventory.cells);
				if (!BlackBoard.cookingDatabase.TryAddIngredient(selectedOption.ingredientData))
                {
                    Debug.LogError("Failed to add ingredient: " + selectedOption.ingredientData.ingredientName);
                    Debug.Break();
                }
                RuntimeManager.PlayOneShot(ingrediantSound);
                break;
            case PickUpType.Fuel:
                BlackBoard.challengeManager.AddTime(selectedOption.fuelCount);
                RuntimeManager.PlayOneShot(feulSound);
                break;
            case PickUpType.Tool:
                BlackBoard.cookingManager.AddSpoon(selectedOption.toolId);
                RuntimeManager.PlayOneShot(toolSound);
                break;
        }
		UIManager.instance.SetVisibleUI(true);
		BlackBoard.playerVFX.pickUp.SendEvent("OnPlay");
		currentIngredient.hasInteracted = true;
		currentIngredient.isActive = false;
		currentIngredient.gameObject.SetActive(false);
		ExitPickUpUI();
    }
    void ExitPickUpUIOnInput(InputAction.CallbackContext context)
    {
		BlackBoard.cookingDatabase.SaveInventory(inventory.cells);
		ExitPickUpUI();
    }
    void ExitPickUpUI()
    {
		//Time.timeScale = 1;
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;

		canvas.enabled = false;
		uIActive = false;

		LeftOption.button.onClick.RemoveAllListeners();
		RightOption.button.onClick.RemoveAllListeners();
		LeftOption.button.interactable = false;
		RightOption.button.interactable = false;
		ClosePickupScreen();
		LeftOption.fuelIcon.SetActive(false);
		LeftOption.toolIcon.SetActive(false);
		RightOption.fuelIcon.SetActive(false);
		RightOption.toolIcon.SetActive(false);

		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Interactions.ExitPickUp, ExitPickUpUIOnInput);
	}
	void GeneratePreview(GameObject ShapePreviewCanvas, IngredientData thisIngredientData)
    {
        if (ShapePreviewCanvas.transform.childCount > 0)
        {
            foreach (Transform child in ShapePreviewCanvas.transform)
            {
                Destroy(child.gameObject);
            }
        }
        //show a visual for the ingredient that correspond with it's shape in the grid
        int[,] ingredientShapeMap = CookingHelperFunctions.GetMapFrom1DArray(thisIngredientData);

        int ingredientWidth = thisIngredientData.rows;
        int ingredientHeight = thisIngredientData.columns;

        for (int x = 0; x < ingredientShapeMap.GetLength(0); x++)
        {
            for (int y = 0; y < ingredientShapeMap.GetLength(1); y++)
            {
                if (ingredientShapeMap[x, y] == 0)
                {
                    //zero (0) represents nothing, emptiness, the void.
                    continue;
                }
                CellData myData = BlackBoard.cookingDatabase.GetCellData(ingredientShapeMap[x, y]);

                //Generate Raw images according to the shape of the ingredient
                GameObject rawImageObject = new GameObject("PreviewCell");

                // Set its parent to the canvas
                rawImageObject.transform.SetParent(ShapePreviewCanvas.transform, false);

                // Add a RawImage component
                rawImageObject.AddComponent<CanvasRenderer>();
                RawImage rawImage = rawImageObject.AddComponent<RawImage>();


                //rawImage.texture = myData.cellTexture;
                rawImage.color = myData.color;

                // Adjust size and position (optional)
                RectTransform rectTransform = rawImageObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(shapePreviewSize, shapePreviewSize);

                //center the whole ingredient horizontally, and build it from bottom to top

                // Center horizontally by shifting by (width - 1) / 2
                float centeredX = (x - (ingredientShapeMap.GetLength(0) - 1) / 2f) * (shapePreviewSize + pieceOffset);

                // Invert Y positioning so it builds from bottom up
                float adjustedY = y * (shapePreviewSize + pieceOffset);

                rectTransform.anchoredPosition = new Vector2(centeredX, adjustedY);
            }
        }
    }


    private void OpenPickupScreen()
    {
        inventory.gameObject.SetActive(true);
        UIManager.instance.CookingHud.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        inventory.LoadIntoGrid(BlackBoard.cookingDatabase.inventoryData);
    }

    private void ClosePickupScreen()
    {
        inventory.gameObject.SetActive(false);
        UIManager.instance.CookingHud.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        //load into cookingDatabase
        ToolTip.instance.OnHoverExit();
		BlackBoard.cookingManager.inventory.LoadIntoGrid(BlackBoard.cookingDatabase.inventoryData);

        if(BlackBoard.playerManager.GetCurrentState().GetType() == typeof(PickUpState))
        {
            BlackBoard.playerManager.SwitchToPreviousState();
        }
    }

    //Initialization & Exiting Cooking State----------------------
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
            Cursor.visible = true;
            if (gridCursor.visible == true)
                gridCursor.Visible(false);

            isCollidingWithGrid = CollidingWithGrid();
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

                if (Input.GetMouseButtonUp(0) && isExtractingWhole)
                {
                    isExtractingWhole = false;
                    //currentExtractingTime = 0;
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
            {
                HandleControllerPieceHolder();
                Cursor.visible = false;
            }
            else
            {
                if (isCollidingWithGrid)
                    currentGridManager.OnHover(onGridPosition);
            }
        }
    }

    //CookingStation Functions------------------------------------

    [Button("Add Random Ingredient")]
    public void AddRandomIngredient()
    {
        BlackBoard.cookingDatabase.TryAddIngredient(UnityEngine.Random.Range(1, BlackBoard.cookingDatabase.ingredientDatas.Count + 1));
    }

    //Piece Functions---------------------------------------------
    private void HandlePiecePlacement()
    {
        if (isCollidingWithGrid)
        {
            if (CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, currentGridManager))
            {
                RuntimeManager.PlayOneShot(placePiece);
                pieceManager.SetPiece(currentGridManager, onGridPosition);
                isHoldingPiece = false;
                if (isKeyboardAndMouse)
                    Cursor.visible = true;

                HandleTooltip();
            }
            else
            {
                if (!currentGridManager.mayExtract || currentGridManager.cells.Count == 0)
                    return;
                FoodCell selectedCell = CookingHelperFunctions.PieceCompatible(pieceManager.cells, onGridPosition, currentGridManager);
                if (selectedCell == null)
                    return;
                RuntimeManager.PlayOneShot(placePiece);
                pieceManager.SwapPieces(currentGridManager, selectedCell, onGridPosition);
                HandleMouseVisual();
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
                RuntimeManager.PlayOneShot(pickUpPiece);
                pieceManager.ExtractPiece(currentGridManager, onGridPosition);
                if (pieceManager.cells.Count > 0)
                {
                    isHoldingPiece = true;
                    pieceHolder.transform.position = pieceManager.originalCenterPosition;
                    HandleMouseVisual();
                }
                ToolTip.instance.OnHoverExit();
            }
            else
            {
                RuntimeManager.PlayOneShot(pickUpPiece);
                pieceManager.ExtractPiece(currentGridManager, onGridPosition);
                if (pieceManager.cells.Count > 0)
                {
                    isHoldingPiece = true;
                    HandleMouseVisual();
                    HandleTooltip();
                }
            }
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
                pieceHolder.MoveObjectToGrid(onGridPosition, currentGridManager, pieceManager.pieceCenterOffset, 1);
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
                {
                    canMove = false;
                }
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
                    Cursor.visible = false;
                    break;
                case CookingStationInteractable.CookingInteractableType.Button:
                    PlaceCursor();
                    break;
                case CookingStationInteractable.CookingInteractableType.Plate:
                    PlaceCursor();
                    break;
                case CookingStationInteractable.CookingInteractableType.Trash:
                    break;
                case CookingStationInteractable.CookingInteractableType.Book:
                    PlaceCursor();
                    break;
                default:
                    break;
            }
            return true;
        }
        else
            return false;
    }

    private void PlaceCursor()
    {
        Vector3 screenPoint = uiCam.WorldToScreenPoint(currentInteractable.gameObject.transform.position);
        Mouse.current.WarpCursorPosition(screenPoint);
        if (isHoldingPiece)
            Cursor.visible = false;
        else
            Cursor.visible = true;
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
                pieceHolder.MoveObjectToGrid(onGridPosition, currentGridManager, pieceManager.pieceCenterOffset, 1);
            else
            {
                if (onGridPosition.x < 0 || onGridPosition.x > currentGridManager.gridPositions.GetLength(0) || onGridPosition.y < 0 || onGridPosition.y > currentGridManager.gridPositions.GetLength(1))
                    return;
                Vector3 lookDirection = (currentGridManager.gridPositions[onGridPosition.x, onGridPosition.y].position - uiCam.transform.position).normalized;
                Vector3 newPosition = currentGridManager.gridPositions[onGridPosition.x, onGridPosition.y].position - lookDirection * aboveGridDistance;
                pieceHolder.MoveObjectAboveGrid(newPosition, currentGridManager.transform.rotation);
            }
        }
        else
        {
            Vector3 lookDirection = (currentInteractable.transform.position - uiCam.transform.position).normalized;
            Ray ray = new Ray(uiCam.transform.position, lookDirection);
            Vector3 newPosition = ray.origin + ray.direction.normalized * offGridDistance;
            Quaternion newRotation = Quaternion.LookRotation(ray.direction);
            pieceHolder.MoveObjectToPoint(newPosition, newRotation);
        }

    }

    //Grid Calculations-------------------------------------------
    private bool CollidingWithGrid()
    {
        Ray ray = uiCam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, gridLayers))
        {
            if (hit.transform.CompareTag("Grid"))
            {
                currentGridManager = hit.transform.GetComponent<GridManager>();
                Vector3 mouseDirection = (hit.point - uiCam.ScreenToWorldPoint(Input.mousePosition)).normalized;
                aboveGridPosition = hit.point - mouseDirection * aboveGridDistance;
                onGridPosition = CookingHelperFunctions.ConvertPointToGrid(hit.point, hit.transform, pieceManager.pieceCenterOffset, cellScale);
                worldPosition = hit.point;
                return true;
            }
        }
        if (currentGridManager != null && currentGridManager.hovering)
            currentGridManager.OnExit();
        return false;
    }
    private void CalculateOffGridPosition()
    {
        Ray ray = uiCam.ScreenPointToRay(Input.mousePosition);
        offGridPosition = ray.origin + ray.direction.normalized * offGridDistance;
        offGridRotation = Quaternion.LookRotation(ray.direction);
    }

    //UI Functions------------------------------------------------
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
            data.title = selectedCell.cellScore.mainTag.GetTagName();
            data.description = selectedCell.cellScore.mainTag.GetTagDescription();
            data.icon = selectedCell.cellScore.mainTag.tagSymbol;
            ToolTip.instance.OnHoverEnter(data);
        }
    }
    public void HandleMouseVisual()
    {
        pieceHolder.transform.position = pieceManager.originalCenterPosition;
        Vector3 screenPoint = uiCam.WorldToScreenPoint(pieceManager.originalCenterPosition);
        Mouse.current.WarpCursorPosition(screenPoint);
        Cursor.visible = false;
    }
}

[System.Serializable]
public class UIOption
{
    public Button button;
    public TMP_Text title;
    public TMP_Text description;
    public GameObject previewContainer;
    public GameObject fuelIcon;
    public GameObject toolIcon;
}
