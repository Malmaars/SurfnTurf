using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class SaveSystem : MonoBehaviour
{
    
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
    public int originalIngredient;

    public FoodCellData(int x, int y, int id, List<Vector2Int> group, int cellTexturePosition, Vector2Int textureGridSize, int originalIngredient)
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
