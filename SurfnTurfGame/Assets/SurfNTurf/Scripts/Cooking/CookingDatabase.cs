using UnityEngine;
using System.Collections.Generic;

public class CookingDatabase : MonoBehaviour
{

    public List<CellData> cellDatas;
    public Dictionary<int, CellData> cellLookupTable;

    public List<IngredientData> ingredientDatas;
    public Dictionary<int, IngredientData> ingredientLookupTable;

    public GridData inventoryData;
    public Vector2Int gridSize;
    public int[,] gridOccupation;
    public bool inventoryChanged;

    private Vector2Int[] groupOffsets = new Vector2Int[]
    {
        new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
        new Vector2Int(-1,  0), new Vector2Int(1,  0),
        new Vector2Int(-1,  1), new Vector2Int(0,  1), new Vector2Int(1,  1),
    };

    private void Awake()
    {
        BlackBoard.cookingDatabase = this;
        UpdateDictionaries();
        //LoadInventoryData();
        gridOccupation = CookingHelperFunctions.GetMapFrom1DArray(inventoryData.foodCells, gridSize.x, gridSize.y);
    }

    private void Update()
    {
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    TryAddIngredient(Random.Range(1, ingredientLookupTable.Count + 1));
        //}
    }

    public void UpdateDictionaries()
    {
        cellLookupTable = new Dictionary<int, CellData>();
        foreach (CellData data in cellDatas)
        {
            if (!cellLookupTable.ContainsKey(data.id))
                cellLookupTable[data.id] = data;
        }

        ingredientLookupTable = new Dictionary<int, IngredientData>();
        foreach (IngredientData data in ingredientDatas)
        {
            if (!ingredientLookupTable.ContainsKey(data.id))
                ingredientLookupTable[data.id] = data;
        }
    }

    public CellData GetCellData(int id)
    {
        return cellLookupTable.TryGetValue(id, out CellData data) ? data : null;
    }

    public IngredientData GetIngredientData(int id)
    {
        return ingredientLookupTable.TryGetValue(id, out IngredientData data) ? data : null;
    }

    public bool TryAddIngredient(int id)
    {

        IngredientData ingredientData = GetIngredientData(id);
        if (ingredientData == null)
            return false;

        PiecePlacementInfo info = GridCompatible(ingredientData);

        if (info.canGoOnGrid)
        {
            SetDataToInventory(info);
            inventoryChanged = true;
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
        info.size = new Vector2Int(data.rows, data.columns);


        int[,] map = CookingHelperFunctions.GetMapFrom1DArray(data.shape, data.rows, data.columns);

        info.offset = CookingHelperFunctions.GetFirstCellPosition(map);


        List<Vector2Int> points = CookingHelperFunctions.MapToPoints(map);

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
                            List<int> texturePositions = CookingHelperFunctions.MapToTexturePositions(map);
                            List<int> IDs = CookingHelperFunctions.MapToIDs(map);

                            info.canGoOnGrid = true;
                            info.points = points;
                            info.texturePositions = texturePositions;
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

    public void SetDataToInventory(PiecePlacementInfo info)
    {
        List<FoodCellData> newCells = new List<FoodCellData>();
        Dictionary<Vector2Int, FoodCellData> cellLookup = new Dictionary<Vector2Int, FoodCellData>();

        for (int i = 0; i < info.points.Count; i++)
        {
            Vector2Int cellPos = info.points[i] - info.offset;
            Vector2Int gridPos = cellPos + info.onGridPosition;

            FoodCellData newCell = new FoodCellData(gridPos.x, gridPos.y, info.IDs[i], new List<Vector2Int>(), 0, Vector2Int.zero);
            gridOccupation[gridPos.x, gridPos.y] = 1;

            newCells.Add(newCell);
            cellLookup[gridPos] = newCell;
        }

        foreach (FoodCellData cell in newCells)
        {
            foreach (Vector2Int offset in groupOffsets)
            {
                Vector2Int neighborPos = new Vector2Int(cell.x, cell.y) + offset;

                if (cellLookup.TryGetValue(neighborPos, out FoodCellData neighbor))
                {
                    cell.group.Add(neighborPos);
                }
            }
        }

        inventoryData.foodCells.AddRange(newCells);
    }

    public void SaveInventory(List<FoodCell> cells)
    {
        List<FoodCellData> newCells = new List<FoodCellData>();

        gridOccupation = new int[gridSize.x, gridSize.y];

        for (int i = 0; i < cells.Count; i++)
        {
            List<Vector2Int> groupCells = new List<Vector2Int>();

            foreach (FoodCell cell in cells[i].groupCells)
            {
                groupCells.Add(cell.gridPosition);
            }
            FoodCellData newCell = new FoodCellData(cells[i].gridPosition.x, cells[i].gridPosition.y, cells[i].cellID, groupCells, 0, Vector2Int.zero);
            gridOccupation[cells[i].gridPosition.x, cells[i].gridPosition.y] = 1;

            newCells.Add(newCell);
        }

        inventoryData.foodCells = newCells;
    }

}

public class PiecePlacementInfo
{
    public Vector2Int onGridPosition;
    public Vector2Int offset;
    public Vector2Int size;
    public List<Vector2Int> points;
    public List<int> IDs;
    public List<int> texturePositions;
    public bool canGoOnGrid;
}
