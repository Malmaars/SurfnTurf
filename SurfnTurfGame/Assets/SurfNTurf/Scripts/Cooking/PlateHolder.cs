using UnityEngine;
using System.Collections.Generic;
public class PlateHolder : MonoBehaviour
{
    public List<FoodCell> mainCells = new List<FoodCell>();
    public List<FoodCell> sideCells = new List<FoodCell>();
    public List<FoodCell> topCells = new List<FoodCell>();

    public GameObject dishSphere;

    public void AddDish(List<FoodCell> _cells)
    {
        mainCells.Clear();
        mainCells.AddRange(_cells);
        foreach (FoodCell cell in mainCells)
        {
            cell.SetParent(transform, true);
            cell.HideCell();
        }
        UpdateDishVisual(true);
    }

    public List<FoodCell> GetCells()
    {
        return mainCells;
    }

    public void ExtractDish()
    {
        foreach (FoodCell cell in mainCells)
        {
            cell.ShowCell();
        }
        mainCells.Clear();
        UpdateDishVisual(false);
    }

    public void UpdateDishVisual(bool active)
    {
        if (active)
        {
            dishSphere.SetActive(active);
        }
        else
        {
            dishSphere.SetActive(active);
        }
    }
}