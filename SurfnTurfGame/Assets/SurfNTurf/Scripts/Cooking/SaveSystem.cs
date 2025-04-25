using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    public CookingManager cookingManager;
    private static string path;

    public void Start()
    {
        path = Application.persistentDataPath + "/gameData.json";
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            Load();
        }
        if (Input.GetKeyDown(KeyCode.K))
        {
            Save();
        }
    }

    public void Save()
    {
        SaveAllGrids(cookingManager.allGrids);
    }

    public void Load()
    {
        LoadAllGridsIntoManagers(cookingManager.allGrids);
    }

    public static void SaveAllGrids(List<GridManager> allGridManagers)
    {
        GameData gameData = new GameData();

        foreach (GridManager gridManager in allGridManagers)
        {
            if (gridManager.canBeSaved)
            {
                GridData gridData = new GridData(gridManager.gridName);

                foreach (FoodCell cell in gridManager.cells)
                {
                    List<Vector2Int> groupCells = new List<Vector2Int>();
                    foreach (FoodCell groupCell in cell.groupCells)
                    {
                        groupCells.Add(groupCell.gridPosition);
                    }
                    gridData.foodCells.Add(new FoodCellData(cell.gridPosition.x, cell.gridPosition.y, cell.cellID, groupCells, cell.cellTexturePosition, cell.textureGridSize, cell.originalIngredient));
                }

                gameData.allGrids.Add(gridData);
            }
        }

        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(path, json);
        Debug.Log("All grids saved to " + path);
    }

    public static GameData LoadAllGrids()
    {
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            GameData gameData = JsonUtility.FromJson<GameData>(json);
            Debug.Log("All grids loaded.");
            return gameData;
        }
        Debug.LogWarning("Save file not found.");
        return null;
    }

    public static void LoadAllGridsIntoManagers(List<GridManager> allGridManagers)
    {
        GameData loadedData = LoadAllGrids();
        if (loadedData == null) return;

        foreach (var gridData in loadedData.allGrids)
        {
            foreach (GridManager grid in allGridManagers)
            {
                grid.ClearGrid();
            }
            GridManager matchingGrid = allGridManagers.Find(g => g.gridName == gridData.gridName);
            if (matchingGrid != null)
            {
                matchingGrid.LoadIntoGrid(gridData);
            }
        }
    }
}

[System.Serializable]
public class GridData
{
    public string gridName;
    public List<FoodCellData> foodCells = new List<FoodCellData>();

    public GridData(string name)
    {
        this.gridName = name;
    }
}

[System.Serializable]
public class FoodCellData
{
    public int x, y;
    public int id;
    public List<Vector2Int> group;
    public int texturePosition;
    public Vector2Int textureGridSize;
    public string originalIngredient;

    public FoodCellData(int x, int y, int id, List<Vector2Int> group, int cellTexturePosition, Vector2Int textureGridSize, string originalIngredient)
    {
        this.x = x;
        this.y = y;
        this.id = id;
        this.group = group;
        this.texturePosition = cellTexturePosition;
        this.textureGridSize = textureGridSize;
        this.originalIngredient = originalIngredient;
    }
}

[System.Serializable]
public class GameData
{
    public List<GridData> allGrids = new List<GridData>();
}
