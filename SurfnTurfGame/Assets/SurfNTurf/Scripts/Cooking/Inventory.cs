using UnityEngine;
using System.Collections.Generic;

public class Inventory : GridManager
{
    [SerializeField]
    private int[,] pieceOccupation = { { 0, 0, 0, 0, 0 }, { 0, 0, 1, 0, 0 }, { 0, 1, 1, 0, 0 }, { 0, 0, 0, 0, 0 }, { 0, 0, 0, 0, 0 } };
    private Vector2Int mapCenter;
    //public GameObject foodCell;
    public bool canPickup;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && canPickup)
        {
            PickupPiece();
        }
    }

    public void PickupPiece()
    {
        IngredientData ingredientData = CookingDatabase.GetIngredientData(Random.Range(1, CookingDatabase.Instance.ingredientDatas.Count+1));
        if (ingredientData == null)
            return;

        PiecePlacementInfo info = GridCompatible(ingredientData);

        if (info.canGoOnGrid)
        {
            GeneratePiecesOnGrid(info);
        }
    }

    /*
    public PiecePlacementInfo GridCompatible()
    {
        PiecePlacementInfo info = new PiecePlacementInfo();
        info.canGoOnGrid = false;

        info.offset = CookingHelperFunctions.GetFirstCellPosition(pieceOccupation);

        List<Vector2Int> points = CookingHelperFunctions.MapToPoints(pieceOccupation);
        List<int> IDs = CookingHelperFunctions.MapToIDs(pieceOccupation);

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (gridOccupation[x, y] == 0)
                {
                    info.onGridPosition = new Vector2Int(x, y);
                    
                    for (int i = 0; i < 4; i++)
                    {
                        if (CookingHelperFunctions.GridCompatible(points, info.offset, gridOccupation, info.onGridPosition))
                        {
                            info.canGoOnGrid = true;
                            info.points = points;
                            info.IDs = IDs;
                            return info;
                        }
                        points = CookingHelperFunctions.RotatePoints(points, info.offset, true);
                    }
                }
            }
        }

        return info;
    }
    */
    
    public PiecePlacementInfo GridCompatible(IngredientData data)
    {
        PiecePlacementInfo info = new PiecePlacementInfo();
        info.canGoOnGrid = false;

        int[,] map = CookingHelperFunctions.GetMapFrom1DArray(data.shape, data.rows, data.columns);

        info.offset = CookingHelperFunctions.GetFirstCellPosition(map);

        List<Vector2Int> points = CookingHelperFunctions.MapToPoints(map);
        List<int> IDs = CookingHelperFunctions.MapToIDs(map);

        for (int y = 0; y < gridSize.y; y++)
        {
            for (int x = 0; x < gridSize.x; x++)
            {
                if (gridOccupation[x, y] == 0)
                {
                    info.onGridPosition = new Vector2Int(x, y);

                    for (int i = 0; i < 4; i++)
                    {
                        if (CookingHelperFunctions.GridCompatible(points, info.offset, gridOccupation, info.onGridPosition))
                        {
                            info.canGoOnGrid = true;
                            info.points = points;
                            info.IDs = IDs;
                            return info;
                        }
                        points = CookingHelperFunctions.RotatePoints(points, info.offset, true);
                    }
                }
            }
        }

        return info;
    }
    
    public void GeneratePiecesOnGrid(PiecePlacementInfo info)
    {
        List<FoodCell> newCells = new List<FoodCell>();

        for (int i = 0; i < info.points.Count; i++)
        {
            Vector2Int cellPos = info.points[i] - info.offset;
            Vector2Int gridPos = cellPos + info.onGridPosition;
            Vector2 worldPos = gridPositions[gridPos.x, gridPos.y].localPosition;

            FoodCell newCell = Instantiate(foodCell).GetComponent<FoodCell>();
            newCell.GenerateFoodCell(gridPos, worldPos, cellHolder, true, info.IDs[i]);
            gridOccupation[gridPos.x, gridPos.y] = 1;
            newCells.Add(newCell);
        }

        foreach (FoodCell cell in newCells)
        {
            cell.SetGroup(newCells);
        }

        cells.AddRange(newCells);
    }
}

public class PiecePlacementInfo
{
    public Vector2Int onGridPosition;
    public Vector2Int offset;
    public List<Vector2Int> points;
    public List<int> IDs;
    public bool canGoOnGrid;
}
