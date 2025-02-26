using UnityEngine;
using System.Collections.Generic;

public class FoodCell : MonoBehaviour
{
    public Vector2Int gridPosition;
    public int cellRotation;
    public GameObject cellVisual;
    public bool onGrid;
    public int cellID;

    public List<FoodCell> groupCells;
    public List<FoodCell> neighborCells;

    public void GenerateFoodCell(Vector2Int _gridPosition, Vector2 _worldPosition, Transform _parent, bool _onGrid, int cellID)
    {
        groupCells = new List<FoodCell>();
        neighborCells = new List<FoodCell>();

        SetParent(_parent, _onGrid);
        SetPosition(_gridPosition, _worldPosition);
        SetCellID(cellID);
        GenerateVisual();
    }

    public void SetParent(Transform _parent, bool _onGrid)
    {
        transform.parent = _parent;
        onGrid = _onGrid;
    }

    public void SetPosition(Vector2Int _gridPosition, Vector2 _worldPosition)
    {
        transform.localPosition = new Vector3(_worldPosition.x, _worldPosition.y, 0);
        transform.localRotation = Quaternion.identity;
        gridPosition = _gridPosition;
        transform.name = gridPosition.ToString();
    }

    public void SetCellID(int _cellID)
    {
        cellID = _cellID;
    }

    public void SetRotation(int _cellRotation)
    {
        cellRotation = _cellRotation;
    }

    public void SetGroup(List<FoodCell> _groupCells)
    {
        groupCells.Clear();
        foreach (FoodCell cell in _groupCells)
        {
            if(cell.gridPosition != gridPosition) groupCells.Add(cell);
        }
    }

    public void SetNeighbors(List<FoodCell> _grid)
    {
        Vector2Int[] neighborOffsets = {
            new Vector2Int(1, 0),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(0, -1)
        };

        neighborCells.Clear();

        foreach (Vector2Int offset in neighborOffsets)
        {
            FoodCell neighborCell = _grid.Find(cell => cell.gridPosition == gridPosition + offset);

            if (neighborCell != null)
            {
                neighborCells.Add(neighborCell);
            }
        }
    }

    public void GenerateVisual()
    {
        cellVisual = Instantiate(cellVisual, transform);
        cellVisual.transform.localPosition += new Vector3(0, 0, -0.1f);
        cellVisual.GetComponent<SpriteRenderer>().color = CookingDatabase.GetCellData(cellID).color;
        UpdateVisual();
    }

    public void UpdateVisual()
    {
        if(onGrid) cellVisual.transform.localPosition = new Vector3(0, 0, -0.1f);
        else cellVisual.transform.localPosition = new Vector3(0, 0, -0.3f);
    }
}
