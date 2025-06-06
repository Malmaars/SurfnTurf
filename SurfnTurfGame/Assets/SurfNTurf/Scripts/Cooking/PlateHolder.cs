using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System.Linq;
using UnityEngine.VFX;
using FMODUnity;

public class PlateHolder : MonoBehaviour
{
    public GameObject dish;
    public Material dishVisual;
    public VisualEffect dishVFX;
    public bool updatingDish;

    [Header("Score Variables")]
    public List<FoodCell> mainCells = new List<FoodCell>();
    private int totalScore;

    private void Awake()
    {
        dishVisual = new Material(dish.GetComponent<MeshRenderer>().material);

        dishVisual.SetInt("_hasMain", 0);
        dishVisual.SetInt("_hasSide", 0);
        dishVisual.SetInt("_hasTopping", 0);

        dish.GetComponent<MeshRenderer>().material = dishVisual;
    }
    public void ServeDish()
    {
        AddDish(BlackBoard.cookingManager.pan.cells);
    }

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
            dishVFX.SendEvent("OnPlay");
            RuntimeManager.PlayOneShot(BlackBoard.cookingManager.serfDish);
            Dictionary<int, int> ingredientCounts = new Dictionary<int, int>();
            foreach (var cell in mainCells)
            {
                int id = cell.originalIngredient;
                if (ingredientCounts.ContainsKey(id))
                    ingredientCounts[id]++;
                else
                    ingredientCounts[id] = 1;
            }

            List<int> sortedIds = ingredientCounts
                .OrderByDescending(kvp => kvp.Value)
                .Select(kvp => kvp.Key)
                .ToList();

            
            if (sortedIds.Count >= 1)
            {
                IngredientData ingredient = BlackBoard.cookingDatabase.GetIngredientData(sortedIds[0]);
                dishVisual.SetInt("_hasMain", 1);
                dishVisual.SetColor("_colorMain", ingredient.ingredientColor);
                dishVisual.SetColor("_colorDarkMain", ingredient.ingredientDarkColor);
            }
            if(sortedIds.Count >= 2)
            {
                IngredientData ingredient = BlackBoard.cookingDatabase.GetIngredientData(sortedIds[1]);
                dishVisual.SetInt("_hasSide", 1);
                dishVisual.SetColor("_colorSide", ingredient.ingredientColor);
                dishVisual.SetColor("_colorDarkSide", ingredient.ingredientDarkColor);
            }
            if (sortedIds.Count >= 3)
            {
                IngredientData ingredient = BlackBoard.cookingDatabase.GetIngredientData(sortedIds[2]);
                dishVisual.SetInt("_hasTopping", 1);
                dishVisual.SetColor("_colorTopping", ingredient.ingredientColor);
                dishVisual.SetColor("_colorDarkTopping", ingredient.ingredientDarkColor);
            }
        }
        else
        {
            dishVisual.SetInt("_hasMain", 0);
            dishVisual.SetInt("_hasSide", 0);
            dishVisual.SetInt("_hasTopping", 0);
        }
        dish.GetComponent<MeshRenderer>().material = dishVisual;
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
            if (cell.originalIngredient == selectedIngredient.id)
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
            if (cell.originalIngredient == selectedIngredient.id)
            {
                bool doesNeighbourIngredient = false;
                foreach (FoodCell neighbour in cell.neighborCells)
                {
                    if (neighbour.originalIngredient == neighbourIngredient.id)
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