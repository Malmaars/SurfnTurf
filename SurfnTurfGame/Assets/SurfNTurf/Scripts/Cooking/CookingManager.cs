using System;
using UnityEngine;
using Unity.Mathematics;

public class CookingManager : MonoBehaviour
{
    [Header("Hand settings")]
    [Range(0.01f,1f)]
    [SerializeField] private float offGridSmoothing;
    [Range(0.01f, 1f)]
    [SerializeField] private float onGridSmoothing;
    [SerializeField] private float offGridDistance = 10;
    public Vector3 offGridPosition;
    public Vector2Int onGridPosition;
    private bool canPlace;
    private bool isHoldingSomething;

    public GameObject pieceHolderObject;
    private PieceHolder pieceHolderScript;
    private PieceManager pieceManager;

    private GridManager currentGridManager;

    private void Start()
    {
        pieceHolderScript = Instantiate(pieceHolderObject).AddComponent<PieceHolder>();
        pieceManager = pieceHolderScript.gameObject.GetComponent<PieceManager>();
        isHoldingSomething = true;
    }

    private void Update()
    {
        if (CheckIfCollidingWithGrid())
        {
            if (CheckIfGridCompatible())
            {
                pieceHolderScript.MoveObjectToGrid(onGridPosition, currentGridManager, onGridSmoothing);
                canPlace = true;
            }
            else
            {
                CalculateHandPosition();
                pieceHolderScript.MoveObjectToMouse(offGridPosition, offGridSmoothing);
                canPlace = false;
            }
        }
        else
        {
            CalculateHandPosition();
            pieceHolderScript.MoveObjectToMouse(offGridPosition, offGridSmoothing);
            canPlace = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!isHoldingSomething && CheckIfCollidingWithGrid())
            {
                if (currentGridManager.lockPiece) pieceManager.ExtractWhole(currentGridManager.ExtractWhole());
                else pieceManager.ExtractPiece(currentGridManager.ExtractPiece(onGridPosition));
            }
            else if(isHoldingSomething && canPlace)
            {
                PlacePiece();
            }
        }
    }

    public void PlacePiece()
    {
        currentGridManager.PlacePiece(pieceManager.cells, onGridPosition);
        pieceManager.cells.Clear();
    }

    public void CalculateHandPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        offGridPosition = ray.origin + ray.direction.normalized * offGridDistance;
    }

    private bool CheckIfCollidingWithGrid()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.transform.tag == "Grid")
            {
                onGridPosition = ConvertMousePosToGridPos(hit.point, hit.transform);
                return true;
            }
        }
        return false;
    }

    public bool CheckIfGridCompatible()
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

    private Vector2Int ConvertMousePosToGridPos(Vector3 point, Transform hitTransform)
    {
        Vector4 tempPos = math.mul(hitTransform.worldToLocalMatrix, new Vector4(point.x, point.y, point.z, 1));
        currentGridManager = hitTransform.parent.GetComponent<GridManager>();
        Vector2Int gridPos = new Vector2Int((int)tempPos.x, (int)tempPos.y);
        return gridPos;
    }
}
