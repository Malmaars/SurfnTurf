using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class Wok : GridManager
{
    private int totalScore;
    public TextMeshProUGUI scoreText;

    public override void SetCells(List<FoodCell> _cells, Vector2Int _onGridPosition)
    {
        base.SetCells(_cells, _onGridPosition);

        totalScore = 0;

        foreach (FoodCell cell in cells)
        {
            cell.CalculateScore();
            totalScore += cell.cellScore.finalScore;
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
}
