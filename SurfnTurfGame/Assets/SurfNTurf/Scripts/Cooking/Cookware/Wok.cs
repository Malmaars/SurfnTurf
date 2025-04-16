using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public class Wok : GridManager
{
    private int totalScore;
    public TextMeshProUGUI scoreText;
    public Coroutine animatingScore;

    public override void SetCells(List<FoodCell> _cells, Vector2Int _onGridPosition)
    {
        base.SetCells(_cells, _onGridPosition);

        totalScore = 0;

        List<FoodCell> scoredCells = new();

        foreach (FoodCell cell in cells)
        {
            if (cell.neighboursHaveChanged || cell.hasCalculatedScore == false)
            {
                cell.CalculateScore(showScore);
                scoredCells.Add(cell);
            }
            totalScore += cell.cellScore.finalScore;
        }

        if(animatingScore != null)
        {
            StopCoroutine(animatingScore);
            animatingScore = StartCoroutine(AnimateScore(scoredCells));
        }
        else
        {
            animatingScore = StartCoroutine(AnimateScore(scoredCells));
        }

        scoreText.text = totalScore.ToString();
    }

    public override void RemoveCells()
    {
        base.RemoveCells();

        totalScore = 0;
        scoreText.text = totalScore.ToString();
    }

    public override void RemoveCells(List<FoodCell> _cells)
    {
        base.RemoveCells(_cells);

        totalScore = 0;

        foreach (FoodCell cell in cells)
        {
            cell.CalculateScore(showScore);
            totalScore += cell.cellScore.finalScore;
        }

        scoreText.text = totalScore.ToString();
    }

    IEnumerator AnimateScore(List<FoodCell> _cells)
    {
        float timeBetweenScore = 1f;
        float timeModifier = 0.5f;

        List<List<CellRulePair>> cellsToAnimate = new();
        foreach (FoodCell cell in _cells)
        {
            if(cell.cellScore.previousScore != cell.cellScore.finalScore && cell.newNeighborCells.Count != 0)
            {
                List<CellRulePair> subCellsToAnimate = new();
                foreach (FoodCell neighbourCell in cell.newNeighborCells)
                {
                    foreach (CellTagRulePair rule in cell.cellScore.mainTag.rules)
                    {
                        if (rule.tag == neighbourCell.cellScore.mainTag)
                        {
                            CellRulePair cellRulePair = new() {cell = cell, rule = rule};
                            subCellsToAnimate.Add(cellRulePair);
                        }
                    }
                }
                cellsToAnimate.Add(subCellsToAnimate);
            }
        }

        foreach (List<CellRulePair> subCells in cellsToAnimate)
        {
            foreach (CellRulePair cellRulePair in subCells)
            {
                cellRulePair.cell.PlayScoreAnimation(cellRulePair.rule.tag);

                yield return new WaitForSeconds(timeBetweenScore);
                timeBetweenScore *= timeModifier;
            }
        }

        yield return null;
    }
}

public struct CellRulePair
{
    public FoodCell cell;
    public CellTagRulePair rule;
}
