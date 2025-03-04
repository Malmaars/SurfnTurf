using UnityEngine;

public class GridShower : MonoBehaviour
{
    public CookingManager cookingManager;
    public void ShowGrid()
    {
        if (cookingManager == null) return;
        cookingManager.ShowGrids();
    }

    public void HideGrid()
    {
        if (cookingManager == null) return;
        cookingManager.HideGrids();
    }
}
