using System;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

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
    private bool gridCursorSet;

    public GridManager inventory;
    public List<GridManager> allGrids = new List<GridManager>();
    public GridManager currentGridManager;

    public LayerMask gridLayers;

    [SerializeField]
    private float offGridDistance = 10;
    [SerializeField]
    private float aboveGridDistance = 1;


    private Vector3 previousMousePosition;
    private Vector3 offGridPosition;
    private quaternion offGridRotation;
    private Vector3 aboveGridPosition;
    private Vector2Int onGridPosition;

    //for playtesting
    public List<GameObject> grids;
    private int gridCounter = 0;
    public override void EnterState()
    {
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, CloseCookingStation);
        base.EnterState();
        cameraController.EnterState();
        cameraController.SetCamera(1);
        playerAnimator.SetBool("Table", true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        gameObject.SetActive(true);
        if(hud != null)
            hud.SetActive(false);
        gameObject.transform.localPosition = player.transform.localPosition;
        ResetGridCursor();
        if(BlackBoard.cookingDatabase.inventoryChanged)
            inventory.LoadIntoGrid(BlackBoard.cookingDatabase.inventoryData);
        //speel animatie van cooking station neerzetten af
    }

    

    public override void ExitState()
    {
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, CloseCookingStation);
        playerAnimator.SetBool("Table", false);
        cameraController.ExitState();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        base.ExitState();
        gameObject.SetActive(false);
        if (hud != null)
            hud.SetActive(true);
        BlackBoard.cookingDatabase.SaveInventory(inventory.cells);
    }

    private void Awake()
    {
        if (!initialized)
        {
            
            //cameraController = FindObjectOfType<CookingCameraController>();
            //player = FindObjectOfType<MovementController>().transform;
            allGrids.AddRange(GameObject.FindObjectsByType<GridManager>(FindObjectsSortMode.None));
            foreach (GridManager grid in allGrids)
            {
                grid.ActivateGrid(cellScale);
            }

            pieceManager.cellScale = cellScale;
            initialized = true;
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
                    if(CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, currentGridManager))
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
            if (currentGridManager != null)
                pieceHolder.MoveObjectAboveGrid(aboveGridPosition, currentGridManager.transform.rotation);
            if (CollidingWithGrid() && Input.GetMouseButtonDown(0))
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
        MoveGridCursor();
    }

    public void CloseCookingStation(InputAction.CallbackContext context)
    {
        BlackBoard.playerManager.SwitchState(typeof(MovementController));
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
        if (allGrids.Count != 0)
        {
            onGridPosition.x = (int)allGrids[0].gridSize.x / 2;
            onGridPosition.y = (int)allGrids[0].gridSize.y / 2;
            currentGridManager = allGrids[0];
            gridCursor.SetPosition(currentGridManager.gridPositions[onGridPosition.x, onGridPosition.y].transform);
            gridCursor.Visible(false);
            gridCursorSet = true;
        }
        else
            gridCursorSet = false;
    }

    private void MoveGridCursor()
    {
        if (!gridCursor.canMove)
            return;
        Vector2Int playerInput = Vector2Int.RoundToInt(InputDistributor.playerInputActions.Cooking.DirectionalInput.ReadValue<Vector2>());

        if (playerInput == Vector2Int.zero)
            return;

        Vector2Int newPos = onGridPosition + playerInput;
        Vector2Int finalPos = newPos;

        if (newPos.x < 0)
        {
            finalPos.x = onGridPosition.x;
        }
        if (newPos.x >= currentGridManager.gridSize.x)
        {
            finalPos.x = onGridPosition.x;
        }
        if (newPos.y < 0 || newPos.y >= currentGridManager.gridSize.y)
        {
            finalPos.y = onGridPosition.y;
        }

        gridCursor.SetPosition(currentGridManager.gridPositions[finalPos.x, finalPos.y]);
        onGridPosition = finalPos;

        if (gridCursor.visible == false)
            gridCursor.Visible(true);
    }

    public void LoadNextGrid()
    {
        gridCounter++;
        if(gridCounter >= grids.Count)
        {
            return;
        }
        grids[gridCounter - 1].SetActive(false);
        grids[gridCounter].SetActive(true);
    }

    private bool CollidingWithGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, gridLayers))
        {
            if (hit.transform.tag == "Grid")
            {
                currentGridManager = hit.transform.GetComponent<GridManager>();
                Vector3 mouseDirection = (hit.point - Camera.main.ScreenToWorldPoint(Input.mousePosition)).normalized; 
                aboveGridPosition = hit.point - mouseDirection * aboveGridDistance;
                onGridPosition = CookingHelperFunctions.ConvertPointToGrid(hit.point, hit.transform, pieceManager.pieceCenterOffset, cellScale);
                return true;
            }
        }
        return false;
    } 
}
