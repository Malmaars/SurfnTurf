using UnityEngine;
using System.Collections.Generic;

public class ServingManager : MonoBehaviour
{
    public DishGrid dishGrid;

    public PlateHolder[] plates;


    private void Update()
    {

    }

    public void OpenServingMenu()
    {

    }

    public void PresentDish()
    {
        dishGrid.ActivateGrid(0);
        ExtractPlate();
    }

    public void ExtractPlate()
    {
        List<FoodCell> cells = new();

        cells.AddRange(plates[0].GetCells());
        plates[0].ExtractDish();

        dishGrid.SetCells(cells, dishGrid.gridCenter);
    }
}
