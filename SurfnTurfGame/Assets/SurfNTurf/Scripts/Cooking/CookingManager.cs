using System;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEditor;

public class CookingManager : MonoBehaviour
{
    [Header("Piece Holder Settings")]
    public PieceHolder pieceHolder;
    public PieceManager pieceManager;
    public Transform pieceAnimationHelper;
    public bool isHoldingSomething;
    public bool isPlayingAnimation;
    public float rotationDuration = 0.25f;
    public float cellScale;


    public List<GridManager> allGrids = new List<GridManager>();
    private GridManager currentGridManager;

    [SerializeField]
    private float offGridDistance = 10;
    [SerializeField]
    private float aboveGridDistance = 1;
    
    private Vector3 offGridPosition;
    private Vector3 aboveGridPosition;
    private Vector2Int onGridPosition;

    private void Start()
    {
        allGrids.AddRange(GameObject.FindObjectsByType<GridManager>(FindObjectsSortMode.None));
        foreach (GridManager grid in allGrids)
        {
            grid.ActivateGrid(cellScale);
        }

        pieceManager.cellScale = cellScale;
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

                if (Input.GetMouseButtonDown(0) && CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, currentGridManager))
                {
                    pieceManager.SetPiece(currentGridManager, onGridPosition);
                    isHoldingSomething = false;
                    Cursor.visible = true;
                }
            }
            if (CollidingWithGrid())
            {
                if(CookingHelperFunctions.GridCompatible(pieceManager.cells, onGridPosition, currentGridManager))
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
                pieceHolder.MoveObjectToPoint(offGridPosition);
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
    }

    private bool CollidingWithGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
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
