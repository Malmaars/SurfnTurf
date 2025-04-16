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
            cell.altered = true;
            if (cell.neighboursHaveChanged || cell.hasCalculatedScore == false)
            {
                cell.CalculateScore();
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
            cell.CalculateScore();
            totalScore += cell.cellScore.finalScore;
        }

        scoreText.text = totalScore.ToString();
    }

    IEnumerator AnimateScore(List<FoodCell> _cells)
    {
        float timeBetweenScore = 0.25f;
        float timeModifier = 0.95f;

        List<List<FoodCell>> cellsToAnimate = new();
        foreach (FoodCell cell in _cells)
        {
            if(cell.cellScore.previousScore != cell.cellScore.finalScore && cell.newNeighborCells.Count != 0)
            {
                List<FoodCell> subCellsToAnimate = new();
                foreach (FoodCell neighbourCell in cell.newNeighborCells)
                {
                    foreach (CellTagRulePair rule in cell.cellScore.mainTag.rules)
                    {
                        if (rule.tag == neighbourCell.cellScore.mainTag)
                        {
                            subCellsToAnimate.Add(cell);
                        }
                    }
                }
                cellsToAnimate.Add(subCellsToAnimate);
            }
        }

        foreach (List<FoodCell> subCells in cellsToAnimate)
        {
            foreach (FoodCell cell in subCells)
            {
                cell.PlayScoreAnimation();

                yield return new WaitForSeconds(timeBetweenScore);
                timeBetweenScore *= timeModifier;
            }
        }

        yield return null;
    }
}
