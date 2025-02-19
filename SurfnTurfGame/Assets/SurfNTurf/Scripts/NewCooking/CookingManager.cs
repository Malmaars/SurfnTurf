using System;
using Unity.Mathematics;
using UnityEngine;

public class CookingManager : MonoBehaviour
{
    public PieceHolder pieceHolder;
    public PieceManager pieceManager;

    public GridManager currentGridManager;

    [SerializeField] private float offGridDistance = 10;
    
    private Vector3 offGridPosition;
    private Vector2Int onGridPosition;

    public bool isHoldingSomething;


    private void Update()
    {
        if (isHoldingSomething)
        {
            if (CollidingWithGrid() && GridCompatible())
            {
                pieceHolder.MoveObjectToGrid(onGridPosition, currentGridManager);
                if (Input.GetMouseButtonDown(0))
                {
                    pieceManager.SetPiece(currentGridManager, onGridPosition);
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

        }
    }

    private bool GridCompatible()
    {
        foreach (FoodCell cell in pieceManager.cells)
        {
            if (cell.gridPosition.x + onGridPosition.x < 0 ||
                cell.gridPosition.x + onGridPosition.x > currentGridManager.gridSize.x - 1 ||
                cell.gridPosition.y + onGridPosition.y < 0 ||
                cell.gridPosition.y + onGridPosition.y > currentGridManager.gridSize.y - 1)
                return false;
            if (currentGridManager.gridOccupation[cell.gridPosition.x + onGridPosition.x, cell.gridPosition.y + onGridPosition.y] == 1)
                return false;
        }
        return true;
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
                onGridPosition = CookingHelperFunctions.ConvertPointToGrid(hit.point, hit.transform);
                return true;
            }
        }
        return false;
    }

    
}
