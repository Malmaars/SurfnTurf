using UnityEngine;

[CreateAssetMenu(fileName = "IngredientData", menuName = "Scriptable Objects/IngredientData")]
public class IngredientData : ScriptableObject
{
    public int id;
    public string ingredientName;

    [Header("Dish Visualization")]
    public Color ingredientColor;
    public Color ingredientDarkColor;

    [Header("Shape")]
    public int rows;
    public int columns;

    //integers define the type of cells. 0 (zero) = no cell
    public int[] shape;

    // For convenience, you can create methods to access the 2D data
    public int GetValue(int x, int y)
    {
        return shape[y * rows + x];
    }

    public void SetValue(int x, int y, int value)
    {
        shape[y * rows + x] = value;
    }

    //public TagEnums.FlavourTag[,] flavourTags;
}
