using UnityEngine;
using System.Collections.Generic;

public class Inventory : GridManager
{
    public bool canPickup;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && canPickup)
        {
            TryAddIngredient(Random.Range(1,BlackBoard.cookingDatabase.ingredientLookupTable.Count+1));
        }
    }

    public bool TryAddIngredient(int id)
    {
        IngredientData ingredientData = BlackBoard.cookingDatabase.GetIngredientData(id);
        if (ingredientData == null)
            return false;

        PiecePlacementInfo info = GridCompatible(ingredientData);

        if (info.canGoOnGrid)
        {
            GeneratePiecesOnGrid(info);
            return true;
        }
        else
        {
            return false;
        }
    }

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
            newCell.GenerateFoodCell(gridPos, worldPos, cellHolder, true, info.IDs[i], cellScale);
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
