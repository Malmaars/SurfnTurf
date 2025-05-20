using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;
using TMPro;

public class FryingPan : GridManager
{
    [Header("FryingPanSettings")]
    public int currentAction = 0;
    public int maxActionsPerStage;
    public Gradient bakingColor;
    public VisualEffect vfx;

    [Header("FireButtonSettings")]
    public bool fireActivating;
    public float fireActivationTime;
    private float fireCurrentTimeActivating;

    [Header("Playtest Settings")]
    public SaveSystem saveSystem;
    public TextMeshProUGUI stageText;

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

    public override void RemoveCells()
    {
        base.RemoveCells();
        TurnOff();
    }


    public void UpdateAction()
    {
        currentAction++;
        stageText.text = $"{currentAction}/{maxActionsPerStage}";
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

        stageText.text = $"{currentAction}/{maxActionsPerStage}";

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
            TurnOn();
        yield return null;
    }

    public override void TurnOn()
    {
        base.TurnOn();
        vfx.SendEvent("OnPlay");
        fireActivating = false;
    }

    public override void TurnOff()
    {
        base.TurnOff();
        vfx.SendEvent("OnStop");
    }
}
