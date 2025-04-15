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
    bool initialized = false;
    [Header("Piece Holder Settings")]
    public PieceHolder pieceHolder;
    public PieceManager pieceManager;
    public Transform pieceAnimationHelper;
    public bool isHoldingSomething;
    public bool isPlayingAnimation;
    public float rotationDuration = 0.25f;
    public float cellScale;

    public Animator playerAnimator;
    public CookingCameraController cameraController;
    public Transform player;
    public GameObject hud;
    public GridCursor gridCursor;
    public float cursorSpeedupTime;
    public float cursorSpeedupTimer;
    private bool gridCursorSet;

    public GridManager inventory;
    public List<GridManager> allGrids = new List<GridManager>();
    public List<CookwareHolder> allCookware = new List<CookwareHolder>();
    private int cookwareIndex;
    public GridManager currentGridManager;
    public PlateHolder currentPlate;
    public int gridIndex;

    public GameObject currentPhysicalButton = null;

    public LayerMask gridLayers;
    public LayerMask PhysicalButtonLayers;

    [SerializeField]
    private float offGridDistance = 10;
    [SerializeField]
    private float aboveGridDistance = 1;


    private Vector3 previousMousePosition;
    private Vector3 offGridPosition;
    private quaternion offGridRotation;
    private Vector3 aboveGridPosition;
    public Vector2Int onGridPosition;
    public Vector3 worldPosition;

    //for playtesting
    public List<GameObject> grids;
    private int gridCounter = 0;
    public float timeToExtractWhole;
    private float currentExtractingTime = 0;
    public bool extractingWhole = false;
    
    public override void InitStateTransitions()
    {
        base.InitStateTransitions();

        transitions.Add(new PlayerStateTransition(typeof(MovementController), () => nextState == typeof(MovementController)));
        transitions.Add(new PlayerStateTransition(typeof(PauseState), () => nextState == typeof(PauseState)));
    }
    
    public override void EnterState()
    {
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, CloseCookingStation);
        base.EnterState();
        cameraController.EnterState();
        cameraController.SetCamera(1);
        //playerAnimator.SetBool("Table", true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameObject.SetActive(true);
        if(hud != null)
            hud.SetActive(false);
        gameObject.transform.localPosition = player.transform.localPosition;
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
        cameraController.ExitState();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        base.ExitState();
        if (hud != null)
            hud.SetActive(true);
        BlackBoard.cookingDatabase.SaveInventory(inventory.cells);
        foreach (GridManager grid in allGrids)
        {
            if (!grid.alwaysOn)
            {
                grid.TurnOff();
            }
        }
        //StopCoroutine(SetSteamCounterStat("time_spent_cooking"));
        gameObject.SetActive(false);
    }



    private void Awake()
    {
        if (!initialized)
        {
            //cameraController = FindObjectOfType<CookingCameraController>();
            //player = FindObjectOfType<MovementController>().transform;
            allGrids.AddRange(transform.GetComponentsInChildren<GridManager>());
            allCookware.AddRange(transform.GetComponentsInChildren<CookwareHolder>());
            foreach (CookwareHolder cookware in allCookware)
            {
                cookware.UnlockCookware(cellScale);
                cookware.HideCookware();
            }
            inventory.ActivateGrid(cellScale);
            pieceManager.cellScale = cellScale;
            initialized = true;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            SwitchCookware(-1);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            SwitchCookware(1);
        }

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
                    if(currentPlate == null && CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, currentGridManager))
                    {
                        pieceManager.SetPiece(currentGridManager, onGridPosition);
                        isHoldingSomething = false;
                        Cursor.visible = true;
                    }
                    if(currentGridManager.extractWhole == false)
                    {
                        FoodCell selectedCell = CookingHelperFunctions.PieceCompatible(pieceManager.cells, onGridPosition, currentGridManager);
                        if (selectedCell != null)
                        {
                            pieceManager.SwapPieces(currentGridManager, selectedCell, onGridPosition);
                            pieceHolder.transform.position = pieceManager.originalCenterPosition;
                            HandleMouseVisual();
                        }
                    }
                    if(currentPlate != null)
                    {
                        pieceManager.SetPlate(currentPlate);
                        isHoldingSomething = false;
                        Cursor.visible = true;
                    }
                }
            }
            if (CollidingWithGrid())
            {
                if (CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, currentGridManager))
                {
                    pieceHolder.MoveObjectToGrid(onGridPosition, currentGridManager, pieceManager.pieceCenterOffset, cellScale);
                }
                else
                {
                    pieceHolder.MoveObjectAboveGrid(aboveGridPosition, currentGridManager.transform.rotation);
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
            //CalculateOffGridPosition();
            //pieceHolder.MoveObjectToPoint(offGridPosition, offGridRotation);
            //if (currentGridManager != null)
            //    pieceHolder.MoveObjectAboveGrid(aboveGridPosition, currentGridManager.transform.rotation);
            if (CollidingWithGrid() && Input.GetMouseButtonDown(0))
            {
                if (currentGridManager.extractWhole)
                {
                    extractingWhole = true;
                    StartCoroutine(ExtractWhole());
                }
                else
                {
                    pieceManager.ExtractPiece(currentGridManager, onGridPosition);
                    if (pieceManager.cells.Count > 0)
                    {
                        isHoldingSomething = true;
                        pieceHolder.transform.position = pieceManager.originalCenterPosition;
                        HandleMouseVisual();
                    }
                }
            }

            if((!CollidingWithGrid() || Input.GetMouseButtonUp(0)) && extractingWhole)
            {
                extractingWhole = false;
                currentExtractingTime = 0;
            }
            
            if(CollidingWithPhysicalButton() && currentPhysicalButton != null)
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

            if(currentPlate != null)
            {
                if (Input.GetMouseButtonDown(0) && currentPlate.mainCells.Count != 0)
                {
                    pieceManager.ExtractPlate(currentPlate);
                    isHoldingSomething = true;
                    HandleMouseVisual();
                }
            }
        }
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
        previousMousePosition = Input.mousePosition;
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

    public void CloseCookingStation(InputAction.CallbackContext context)
    {
        nextState = typeof(MovementController);
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

    public void HandleMouseVisual()
    {
        Vector3 screenPoint = Camera.main.WorldToScreenPoint(pieceManager.originalCenterPosition);
        Mouse.current.WarpCursorPosition(screenPoint);
        //Cursor.visible = false;
    }

    IEnumerator ExtractWhole()
    {
        while (currentExtractingTime < timeToExtractWhole)
        {
            if (extractingWhole)
            {
                currentExtractingTime += Time.deltaTime;
            }
            yield return new WaitForEndOfFrame();
        }

        if (extractingWhole)
        {
            pieceManager.ExtractPiece(currentGridManager, onGridPosition);
            if (pieceManager.cells.Count > 0)
            {
                isHoldingSomething = true;
                pieceHolder.transform.position = pieceManager.originalCenterPosition;
                HandleMouseVisual();
            }
            extractingWhole = false;
        }
        
        yield return null;
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

    public void OnDrawGizmos()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(ray);
        Gizmos.DrawSphere(worldPosition, 0.01f);
    }
}
