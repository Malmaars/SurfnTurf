using UnityEngine;

public class GridShower : MonoBehaviour
{
    public CookingManager cookingManager;
    public void ShowGrid()
    {
        cookingManager.ShowGrids();
    }

    public void HideGrid()
    {
        cookingManager.HideGrids();
    }
}
