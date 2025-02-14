using System;
using UnityEngine;
public class FoodCellVisual : MonoBehaviour
{
    public void GenerateVisual(GameObject cellVisual)
    {
        Instantiate(cellVisual, transform);
    }
}