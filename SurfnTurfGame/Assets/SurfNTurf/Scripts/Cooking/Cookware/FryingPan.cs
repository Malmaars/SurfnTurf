using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FryingPan : GridManager
{
    [Header("FryingPanSettings")]
    public int currentAction = 0;
    public int maxActionsPerStage;
    public Gradient bakingColor;

    [Header("FireButtonSettings")]
    public bool fireActivating;
    public float fireActivationTime;
    private float fireCurrentTimeActivating;

    [Header("Playtest Settings")]
    public SaveSystem saveSystem;

    public override void SetCells(List<FoodCell> _cells, Vector2Int _onGridPosition)
    {
        base.SetCells(_cells, _onGridPosition);

        foreach (FoodCell cell in cells)
        {
            cell.SetGroup(cells);
            cell.UpdateVisual();
        }

        UpdateAction();
    }


    public void UpdateAction()
    {
        currentAction++;
        if (currentAction >= maxActionsPerStage)
        {
            currentAction = 0;
            UpdateStage();
        }
    }

    public void UpdateStage()
    {
        StartCoroutine(UpdateStageVisual());
    }

    IEnumerator UpdateStageVisual()
    {
        float duration = 1f; // Duration of baking
        float elapsedTime = 0f;
        

        // Transition to baked color
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;

            Color selectedColor = bakingColor.Evaluate(t);

            foreach (GameObject gridCellVisual in gridCellVisuals)
            {
                Material gridCellVisualMaterial = gridCellVisual.GetComponent<MeshRenderer>().material;
                gridCellVisualMaterial.color = selectedColor;
            }

            yield return null;
        }

        foreach (FoodCell cell in cells)
        {
            if (!cell.burned)
            {

                cell.Bake();
            }
        }

        foreach (GameObject gridCellVisual in gridCellVisuals)
        {
            Material gridCellVisualMaterial = gridCellVisual.GetComponent<MeshRenderer>().material;
            gridCellVisualMaterial.color = bakingColor.Evaluate(0);
        }
        
        yield return null;
    }

    public void ActivateFire(bool firing)
    {
        Debug.Log("Firing");
        if (turnedOn)
        {
            return;
        }
        if (firing)
        {
            fireActivating = firing;
            StartCoroutine(Firing());
        }
        else
        {
            fireActivating = firing;
            StopCoroutine(Firing());
        }
    }

    IEnumerator Firing()
    {
        fireCurrentTimeActivating = 0;
        while (fireCurrentTimeActivating < fireActivationTime)
        {
            fireCurrentTimeActivating += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }

        if (fireActivating)
            TurnOnFryingPan(true);
        yield return null;
    }

    public void TurnOnFryingPan(bool turnOn)
    {
        if (turnOn)
        {
            turnedOn = true;
            saveSystem = FindObjectOfType<SaveSystem>();
            saveSystem.Save();
            fireActivating = false;
        }
        else
        {
            turnedOn = false;
        }
    }
}
