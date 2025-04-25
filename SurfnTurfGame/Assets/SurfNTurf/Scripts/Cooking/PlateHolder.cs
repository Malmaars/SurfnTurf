using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class PlateHolder : MonoBehaviour
{

    public GameObject dishSphere;
    public bool updatingDish;

    [Header("Score Variables")]
    public List<FoodCell> mainCells = new List<FoodCell>();
    private int totalScore;

    //Dish Setting, Removing and Visual
    public void AddDish(List<FoodCell> _cells)
    {
        mainCells.Clear();
        mainCells.AddRange(_cells);
        foreach (FoodCell cell in mainCells)
        {
            cell.SetParent(transform, true);
            cell.HideCell();
        }

        totalScore = 0;
        foreach (FoodCell cell in mainCells)
        {
            cell.CalculateScore(false);
            totalScore += cell.cellScore.finalScore;
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
    public void ClearDish()
    {
        foreach (FoodCell cell in mainCells)
            Destroy(cell);
        mainCells.Clear();
    }

    //Dish Score Functions
    public int GetTotalScore()
    {
        return totalScore;
    }
    //Tag Functions
    public int GetTagAmmount(CellTag selectedTag)
    {
        int ammount = 0;

        foreach (FoodCell cell in mainCells)
        {
            if(cell.cellScore.mainTag == selectedTag)
            {
                ammount++;
            }
        }

        return ammount;
    }
    public float GetTagPercentage(CellTag selectedTag)
    {
        float percentage = 0;
        int totalAmmountOfSelectedTag = GetTagAmmount(selectedTag);

        percentage = (totalAmmountOfSelectedTag / mainCells.Count) * 100;

        return percentage;
    }
    public float GetNeighbouringTagPercentage(CellTag selectedTag, CellTag neighbourTag)
    {
        float percentage = 0;
        int totalAmmountOfSelectedTag = GetTagAmmount(selectedTag);
        int neighbouringTags = 0;

        foreach (FoodCell cell in mainCells)
        {
            if (cell.cellScore.mainTag == selectedTag)
            {
                bool doesNeighbourTag = false;
                foreach (FoodCell neighbour in cell.neighborCells)
                {
                    if(neighbour.cellScore.mainTag == neighbourTag)
                    {
                        doesNeighbourTag = true;
                    }
                }
                if (doesNeighbourTag)
                    neighbouringTags++;
            }
        }

        percentage = (neighbouringTags / totalAmmountOfSelectedTag) * 100;

        return percentage;
    }
    //Ingredient Functions
    public int GetIngredientAmmount(IngredientData selectedIngredient)
    {
        int ammount = 0;

        foreach (FoodCell cell in mainCells)
        {
            if (cell.originalIngredient == selectedIngredient.ingredientName)
            {
                ammount++;
            }
        }

        return ammount;
    }
    public float GetIngredientPercentage(IngredientData selectedIngredient)
    {
        float percentage = 0;
        int totalAmmountOfSelectedIngredient = GetIngredientAmmount(selectedIngredient);

        percentage = (totalAmmountOfSelectedIngredient / mainCells.Count) * 100;

        return percentage;
    }
    public float GetNeighbouringIngredientPercentage(IngredientData selectedIngredient, IngredientData neighbourIngredient)
    {
        float percentage = 0;
        int totalAmmountOfSelectedIngredient = GetIngredientAmmount(selectedIngredient);
        int neighbouringIngredients = 0;

        foreach (FoodCell cell in mainCells)
        {
            if (cell.originalIngredient == selectedIngredient.ingredientName)
            {
                bool doesNeighbourIngredient = false;
                foreach (FoodCell neighbour in cell.neighborCells)
                {
                    if (neighbour.originalIngredient == neighbourIngredient.ingredientName)
                    {
                        doesNeighbourIngredient = true;
                    }
                }
                if (doesNeighbourIngredient)
                    neighbouringIngredients++;
            }
        }

        percentage = (neighbouringIngredients / totalAmmountOfSelectedIngredient) * 100;

        return percentage;
    }
    //
}