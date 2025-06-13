using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using FMODUnity;

public class DishGrid : GridManager
{
    public Vector2Int gridCenter;
    [SerializeField] private int currentScore;
    [SerializeField] private int baseScore;
    public TextMeshProUGUI scoreText;
    public Coroutine animatingScore;
    public AnimationCurve positiveWiggleScore;
    public AnimationCurve negativeWiggleScore;
    public AnimationCurve positiveScalingScore;
    public AnimationCurve negativeScalingScore;
    public Gradient positiveColorScore;
    public Gradient negativeColorScore;

    public override void SetCells(List<FoodCell> _cells, Vector2Int _onGridPosition)
    {
        base.SetCells(_cells, _onGridPosition);

        currentScore = totalScore;
        totalScore = 0;
        baseScore = 0;
        List<FoodCell> scoredCells = new();

        foreach (FoodCell cell in cells)
        {
            cell.SetScale(cellScale);
            cell.CalculateScore(showScore);
            scoredCells.Add(cell);
            totalScore += cell.cellScore.finalScore;
            baseScore += cell.cellScore.baseScore;
        }

        if (animatingScore != null)
        {
            StopCoroutine(animatingScore);
            animatingScore = StartCoroutine(AnimateScore(scoredCells));
        }
        else
        {
            animatingScore = StartCoroutine(AnimateScore(scoredCells));
        }
    }

    IEnumerator AnimateScore(List<FoodCell> _cells)
    {
        float timeBetweenScore = 0.5f;
        float timeModifier = 0.8f;

        /*
        List<List<CellRulePair>> cellsToAnimate = new();
        foreach (FoodCell cell in _cells)
        {
            List<CellRulePair> subCellsToAnimate = new();
            foreach (FoodCell neighbourCell in cell.neighborCells)
            {
                foreach (CellTagRulePair rule in cell.cellScore.mainTag.rules)
                {
                    if (rule.tag == neighbourCell.cellScore.mainTag)
                    {
                        CellRulePair cellRulePair = new() { cell = cell, rule = rule };
                        subCellsToAnimate.Add(cellRulePair);
                    }
                }
            }
            cellsToAnimate.Add(subCellsToAnimate);
        }

        ChangeScore(baseScore, 0.5f, 1f + (Mathf.Abs(baseScore) / 50f));

        yield return new WaitForSeconds(0.5f);

        foreach (List<CellRulePair> subCells in cellsToAnimate)
        {
            foreach (CellRulePair cellRulePair in subCells)
            {
                cellRulePair.cell.PlayScoreAnimation(cellRulePair.rule.tag, timeBetweenScore * 2);
                int score = cellRulePair.rule.rule.Calculate(cellRulePair.rule.value);

                float modifier = 1f + (Mathf.Abs(score) / 10f);

                ChangeScore(score, timeBetweenScore, modifier);

                yield return new WaitForSeconds(timeBetweenScore);
                timeBetweenScore *= timeModifier;
            }
        }
        */
        yield return null;
    }

    public void ChangeScore(int score, float timeToAnimate, float modifier)
    {
        currentScore += score;
        scoreText.text = currentScore.ToString();

        float audioModifier = 1 - (timeToAnimate * 2);

        if (score > 0)
        {
            StartCoroutine(AnimateScore(true, timeToAnimate, modifier));
            FMOD.Studio.EventInstance myEvent;
            myEvent = FMODUnity.RuntimeManager.CreateInstance("event:/SoundEffects/ScoreUp");
            myEvent.setParameterByName("ScoreUpPitchControll", audioModifier); //go to 1 for every scoring
            myEvent.start();
            myEvent.release();

        }
        else if (score < 0)
        {
            StartCoroutine(AnimateScore(false, timeToAnimate, modifier));
            FMOD.Studio.EventInstance myEvent;
            myEvent = FMODUnity.RuntimeManager.CreateInstance("event:/SoundEffects/ScoreDown");
            myEvent.setParameterByName("ScoreDownPitchControll", audioModifier); //go to 1 for every scoring
            myEvent.start();
            myEvent.release();
        }
    }

    IEnumerator AnimateScore(bool positive, float timeToAnimate, float modifier)
    {
        float currentTime = 0f;

        RectTransform textTransform = scoreText.gameObject.GetComponent<RectTransform>();

        while (currentTime < timeToAnimate)
        {
            currentTime += Time.deltaTime;

            float wiggleCurveValue = 0;
            float scalingCurveValue = 0;
            Color colorCurveValue = Color.white;
            if (positive)
            {
                wiggleCurveValue = positiveWiggleScore.Evaluate(currentTime / timeToAnimate);
                scalingCurveValue = positiveScalingScore.Evaluate(currentTime / timeToAnimate);
                colorCurveValue = positiveColorScore.Evaluate(currentTime / timeToAnimate);
            }
            else
            {
                wiggleCurveValue = negativeWiggleScore.Evaluate(currentTime / timeToAnimate);
                scalingCurveValue = negativeScalingScore.Evaluate(currentTime / timeToAnimate);
                colorCurveValue = negativeColorScore.Evaluate(currentTime / timeToAnimate);
            }

            textTransform.localRotation = Quaternion.Euler(0, 0, wiggleCurveValue * modifier);
            textTransform.localScale = new Vector3(scalingCurveValue * modifier, scalingCurveValue * modifier, 1);
            scoreText.color = colorCurveValue;
            yield return new WaitForEndOfFrame();
        }

        scoreText.color = Color.white;
        textTransform.localRotation = Quaternion.identity;
        textTransform.localScale = Vector3.one;
        yield return null;
    }
}
