using UnityEngine;
using System.Collections.Generic;

public class PieceManager : MonoBehaviour
{
    [SerializeField]
    private int[,] pieceOccupation = { { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 }, { 0, 1, 1, 0, 0 }, { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 } };
    public List<FoodCell> cells;
    private Vector2Int mapCenter;
    public GameObject foodCell;
    private bool offsetSet;

    private void Start()
    {
        cells = new List<FoodCell>();
        //GeneratePiece();
    }

    public void SetPiece(GridManager _currentGridManager, Vector2Int _onGridPosition)
    {
        _currentGridManager.SetCells(cells, _onGridPosition);
        cells.Clear();
    }
    public void ExtractPiece(GridManager _currentGridManager, Vector2Int _onGridPosition)
    {
        FoodCell selectedCell = _currentGridManager.cells.Find(cell => cell.gridPosition == _onGridPosition);
        if (selectedCell == null) return;

        if (_currentGridManager.extractWhole)
        {
            cells.AddRange(_currentGridManager.cells);
            _currentGridManager.RemoveCells();
        }
        else
        {
            cells.Add(selectedCell);
            cells.AddRange(selectedCell.groupCells);
            _currentGridManager.RemoveCells(cells);
        }

        Vector2Int pieceCenter = CookingHelperFunctions.GetMapCenter(cells);

        foreach (FoodCell cell in cells)
        {
            Vector2Int gridPos = cell.gridPosition - pieceCenter;
            cell.SetParent(transform, false);
            cell.SetPosition(gridPos, gridPos);
            cell.UpdateVisual();
        }
    }

    public void RotatePiece(bool clockwise)
    {
        foreach (FoodCell cell in cells)
        {
            Vector2Int newPos = CookingHelperFunctions.RotatePosition(cell.gridPosition ,Vector2Int.zero, clockwise);
            cell.SetPosition(newPos, newPos);
        }
    }

    
}