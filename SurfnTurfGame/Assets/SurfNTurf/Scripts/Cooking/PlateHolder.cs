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
        dishSphere.SetActive(active);
    }
    public void ClearDish()
    {
        foreach (FoodCell cell in mainCells)
            Destroy(cell);
        mainCells.Clear();
        UpdateDishVisual(false);
    }

    //Dish Score Functions
    public int GetTotalScore()
    {
        return totalScore;
    }
    //Tag Functions
    public int GetTagAmount(CellTag selectedTag)
    {
        int amount = 0;

        foreach (FoodCell cell in mainCells)
        {
            if(cell.cellScore.mainTag == selectedTag)
            {
                amount++;
            }
        }
        Debug.Log(amount);
        return amount;
    }
    public float GetTagPercentage(CellTag selectedTag)
    {
        float percentage = 0;
        float totalAmountOfSelectedTag = GetTagAmount(selectedTag);
        percentage = (totalAmountOfSelectedTag / mainCells.Count) * 100f;
        return percentage;
    }
    public float GetNeighbouringTagPercentage(CellTag selectedTag, CellTag neighbourTag)
    {
        float percentage = 0;
        float totalAmountOfSelectedTag = GetTagAmount(selectedTag);
        float neighbouringTags = 0;

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

        percentage = (neighbouringTags / totalAmountOfSelectedTag) * 100f;

        return percentage;
    }
    //Ingredient Functions
    public int GetIngredientAmount(IngredientData selectedIngredient)
    {
        int amount = 0;

        foreach (FoodCell cell in mainCells)
        {
            if (cell.originalIngredient == selectedIngredient.ingredientName)
            {
                amount++;
            }
        }

        return amount;
    }
    public float GetIngredientPercentage(IngredientData selectedIngredient)
    {
        float percentage = 0;
        float totalAmountOfSelectedIngredient = GetIngredientAmount(selectedIngredient);

        percentage = (totalAmountOfSelectedIngredient / mainCells.Count) * 100f;

        return percentage;
    }
    public float GetNeighbouringIngredientPercentage(IngredientData selectedIngredient, IngredientData neighbourIngredient)
    {
        float percentage = 0;
        float totalAmountOfSelectedIngredient = GetIngredientAmount(selectedIngredient);
        float neighbouringIngredients = 0;

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

        percentage = (neighbouringIngredients / totalAmountOfSelectedIngredient) * 100f;

        return percentage;
    }
    //
}