using UnityEngine;
using System.Collections.Generic;

public class ServingManager : MonoBehaviour
{
    public bool presentDish;
    public DishGrid dishGrid;
    public PlateHolder plate;

    private void Update()
    {
        if (presentDish)
        {
            presentDish = false;
            PresentDish();
        }
    }

    public void PresentDish()
    {
        dishGrid.ActivateGrid(0);
        ExtractPlate();
    }

    public void ExtractPlate()
    {
        List<FoodCell> cells = new();

        cells.AddRange(plate.GetCells());
        plate.ExtractDish();

        dishGrid.SetCells(cells, dishGrid.gridCenter);
    }
}
