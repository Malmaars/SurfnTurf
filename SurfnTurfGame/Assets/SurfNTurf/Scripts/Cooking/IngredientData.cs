using UnityEngine;

[CreateAssetMenu(fileName = "IngredientData", menuName = "Scriptable Objects/IngredientData")]
public class IngredientData : ScriptableObject
{
    public int id;
    public string ingredientName;

    public int rows;
    public int columns;

    public int[] shape;

    // For convenience, you can create methods to access the 2D data
    public int GetValue(int x, int y, int width)
    {
        return shape[y * width + x];
    }

    public void SetValue(int x, int y, int width, int value)
    {
        shape[y * width + x] = value;
    }

    //public TagEnums.FlavourTag[,] flavourTags;
}
