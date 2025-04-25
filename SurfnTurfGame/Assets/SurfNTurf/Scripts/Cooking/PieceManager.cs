using UnityEngine;
using System.Collections.Generic;
using Steamworks;

public class PieceManager : MonoBehaviour
{
    [SerializeField]
    private int[,] pieceOccupation = { { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 }, { 0, 1, 1, 0, 0 }, { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 } };
    public List<FoodCell> cells;
    public Vector2 pieceCenterOffset;
    public GameObject foodCell;
    public Vector3 originalCenterPosition;
    private bool offsetSet;
    public float cellScale;

    private void Start()
    {
        cells = new List<FoodCell>();
        //GeneratePiece();
    }

    public void SetPiece(GridManager _currentGridManager, Vector2Int _onGridPosition)
    {
        _currentGridManager.SetCells(cells, _onGridPosition);
        cells.Clear();
        pieceCenterOffset = Vector2.zero;
        SetSteamStat("ingredient_moved");
    }

    public void SetPlate(PlateHolder _currentPlate)
    {
        if (_currentPlate.mainCells.Count != 0)
            return;
        _currentPlate.AddDish(cells);
        cells.Clear();
        pieceCenterOffset = Vector2.zero;
    }

    public void RemoveDish()
    {
        foreach (FoodCell cell in cells)
        {
            Destroy(cell.gameObject);
        }
        cells.Clear();
        pieceCenterOffset = Vector2.zero;
    }

    //pick up a piece from the grid
    public void ExtractPiece(GridManager _currentGridManager, Vector2Int _onGridPosition)
    {
        FoodCell selectedCell = _currentGridManager.cells.Find(cell => cell.gridPosition == _onGridPosition);
        if (selectedCell == null) 
            return;

        if (_currentGridManager.extractWhole)
        {
            cells.AddRange(_currentGridManager.cells);
            _currentGridManager.RemoveCells();
        }
        else
        {
            cells.Add(selectedCell);
            List<FoodCell> groupCells = CookingHelperFunctions.ExtractConnectedCells(selectedCell);
            cells.AddRange(groupCells);
            _currentGridManager.RemoveCells(cells);
        }

        Vector2Int pieceCenter = CookingHelperFunctions.GetMapCenter(cells);
        pieceCenterOffset = CookingHelperFunctions.GetPreciseCenter(cells);
        originalCenterPosition = CookingHelperFunctions.GetWorldCenterFromPoints(cells);

        foreach (FoodCell cell in cells)
        {
            Vector2Int gridPos = cell.gridPosition - pieceCenter;
            Vector2 worldPos = (gridPos - pieceCenterOffset) * cellScale;
            cell.SetParent(transform, false);
            cell.SetPosition(gridPos, worldPos);
        }
        foreach (FoodCell cell in cells)
        {
            cell.SetNeighbors(cells);
            cell.CalculateScore(false);
            cell.UpdateVisual();
            cell.PlayEffect("OnHold");
        }
    }

    public void ExtractPlate(PlateHolder _currentPlate)
    {
        if (_currentPlate.updatingDish)
            return;

        cells.AddRange(_currentPlate.GetCells());
        _currentPlate.ExtractDish();

        Vector2Int pieceCenter = CookingHelperFunctions.GetMapCenter(cells);
        pieceCenterOffset = CookingHelperFunctions.GetPreciseCenter(cells);
        originalCenterPosition = CookingHelperFunctions.GetWorldCenterFromPoints(cells);

        foreach (FoodCell cell in cells)
        {
            Vector2Int gridPos = cell.gridPosition - pieceCenter;
            Vector2 worldPos = (gridPos - pieceCenterOffset) * cellScale;
            cell.SetParent(transform, false);
            cell.SetPosition(gridPos, worldPos);
        }
        foreach (FoodCell cell in cells)
        {
            cell.SetNeighbors(cells);
            cell.CalculateScore(false);
            cell.UpdateVisual();
            cell.PlayEffect("OnHold");
        }
    }

    public void SwapPieces(GridManager _currentGridManager, FoodCell _selectedCell, Vector2Int _onGridPosition)
    {
        List<FoodCell> tempCells = new List<FoodCell>();

        if (_currentGridManager.extractWhole)
        {
            tempCells.AddRange(_currentGridManager.cells);
            _currentGridManager.RemoveCells();
        }
        else
        {
            tempCells.Add(_selectedCell);
            List<FoodCell> groupCells = CookingHelperFunctions.ExtractConnectedCells(_selectedCell);
            tempCells.AddRange(groupCells);
            _currentGridManager.RemoveCells(tempCells);
        }

        SetPiece(_currentGridManager, _onGridPosition);
        cells = tempCells;

        Vector2Int pieceCenter = CookingHelperFunctions.GetMapCenter(cells);
        pieceCenterOffset = CookingHelperFunctions.GetPreciseCenter(cells);
        originalCenterPosition = CookingHelperFunctions.GetWorldCenterFromPoints(cells);

        foreach (FoodCell cell in cells)
        {
            Vector2Int gridPos = cell.gridPosition - pieceCenter;
            Vector2 worldPos = (gridPos - pieceCenterOffset) * cellScale;
            cell.SetParent(transform, false);
            cell.SetPosition(gridPos, worldPos);
        }
        foreach (FoodCell cell in cells)
        {
            cell.SetNeighbors(cells);
            cell.CalculateScore(false);
            cell.UpdateVisual();
        }
        SetSteamStat("ingredient_swapped");
    }

    public void RotatePiece(bool clockwise)
    {
        Vector2 tempOffset = new Vector2(pieceCenterOffset.y, pieceCenterOffset.x);
        foreach (FoodCell cell in cells)
        {
            Vector2Int newPos = CookingHelperFunctions.RotatePosition(cell.gridPosition , pieceCenterOffset, clockwise);

            Vector2 worldPos = (newPos - tempOffset) * cellScale;
            cell.SetPosition(newPos, worldPos);
        }
        pieceCenterOffset = tempOffset;
        foreach (FoodCell cell in cells)
        {
            cell.UpdateVisual();
        }
        SetSteamStat("ingredient_turned");
    }

    public void SetSteamStat(string statName)
    {
        if (SteamManager.Initialized)
        {
            SteamUserStats.GetStat(statName, out int statValue);
            statValue++;
            SteamUserStats.SetStat(statName, statValue);
            SteamUserStats.StoreStats();
        }
    }
}