using UnityEngine;
using System.Collections.Generic;

public class CookingDatabase : MonoBehaviour
{

    public List<CellData> cellDatas;
    public Dictionary<int, CellData> cellLookupTable;

    public List<IngredientData> ingredientDatas;
    public Dictionary<int, IngredientData> ingredientLookupTable;

    private void Awake()
    {
        BlackBoard.cookingDatabase = this;
        UpdateDictionaries();
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
}
