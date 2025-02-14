using UnityEngine;
using System.Collections.Generic;

public class FoodCellNew : MonoBehaviour
{
    public Vector2Int gridPosition;
    public int cellRotation;
    public GameObject cellVisual;

    public List<FoodCellNew> groupCells;
    public List<FoodCellNew> neighborCells;

    public FoodCellNew(Vector2Int _gridPosition, Transform _parent)
    {
        groupCells = new List<FoodCellNew>();
        neighborCells = new List<FoodCellNew>();

        SetParent(_parent);
        SetPosition(_gridPosition);
    }

    public void SetParent(Transform _parent)
    {
        transform.parent = _parent;
    }

    public void SetPosition(Vector2Int _gridPosition)
    {
        transform.localPosition = new Vector3(_gridPosition.x, _gridPosition.y, 0);
        gridPosition = _gridPosition;
    }

    public void SetRotation(int _cellRotation)
    {
        cellRotation = _cellRotation;
    }

    public void SetGroup(List<FoodCellNew> _groupCells)
    {
        groupCells.Clear();
        groupCells.AddRange(_groupCells);
    }

    public void SetNeighbors(List<FoodCellNew> _grid)
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
            FoodCellNew neighborCell = _grid.Find(cell => cell.gridPosition == gridPosition + offset);

            if (neighborCell != null)
            {
                neighborCells.Add(neighborCell);
            }
        }
    }

    public void GenerateVisual()
    {
        Instantiate(cellVisual, transform);
    }

    public void UpdateVisual()
    {
        //cellVisual.GetComponent<FoodCellVisual>().UpdateVisual();
    }
}
