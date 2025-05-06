using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using FMODUnity;
using UnityEngine.VFX;

public class Wok : GridManager
{
    [SerializeField] private int currentScore;
    [SerializeField] private int totalScore;
    [SerializeField] private int baseScore;
    public TextMeshProUGUI scoreText;
    public Coroutine animatingScore;
    public AnimationCurve positiveWiggleScore;
    public AnimationCurve negativeWiggleScore;
    public AnimationCurve positiveScalingScore;
    public AnimationCurve negativeScalingScore;
    public Gradient positiveColorScore;
    public Gradient negativeColorScore;

    [Header("FryingPanSettings")]
    public VisualEffect vfx;
    public GameObject spoonSelection;
    public Transform spoonPivot;
    public Transform spoonVisual;
    public float stirDuration;
    private bool spoonActive;
    private bool isStirring;
    private Vector2Int[] spoonOffsets = {
        new Vector2Int(0, 0),
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(1, 1)
    };

    [Header("FireButtonSettings")]
    public bool fireActivating;
    public float fireActivationTime;
    private float fireCurrentTimeActivating;

    public override void ActivateGrid(float _cellScale)
    {
        base.ActivateGrid(_cellScale);
        spoonVisual.localScale = new Vector3(cellScale, cellScale, 1f);
    }

    public override void SetCells(List<FoodCell> _cells, Vector2Int _onGridPosition)
    {
        base.SetCells(_cells, _onGridPosition);

        currentScore = totalScore;
        totalScore = 0;
        baseScore = 0;
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

        foreach (FoodCell cell in _cells)
        {
            baseScore += cell.cellScore.previousScore;
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

        int previousScore = totalScore;
        totalScore = 0;

        foreach (FoodCell cell in cells)
        {
            cell.CalculateScore(showScore);
            totalScore += cell.cellScore.finalScore;
        }

        int difference = totalScore - previousScore;
        ChangeScore(difference, 0.5f, 1f + (Mathf.Abs(difference) / 25f));
    }

    IEnumerator AnimateScore(List<FoodCell> _cells)
    {
        float timeBetweenScore = 0.5f;
        float timeModifier = 0.8f;

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

        ChangeScore(baseScore, 0.5f, 1f + (Mathf.Abs(baseScore) / 50f));

        yield return new WaitForSeconds(0.5f);

        foreach (List<CellRulePair> subCells in cellsToAnimate)
        {
            foreach (CellRulePair cellRulePair in subCells)
            {
                cellRulePair.cell.PlayScoreAnimation(cellRulePair.rule.tag, timeBetweenScore*2);
                int score = cellRulePair.rule.rule.Calculate(cellRulePair.rule.value);

                float modifier = 1f + (Mathf.Abs(score) / 10f);

                ChangeScore(score, timeBetweenScore, modifier);

                yield return new WaitForSeconds(timeBetweenScore);
                timeBetweenScore *= timeModifier;
            }
        }

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

        while(currentTime < timeToAnimate)
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

    public void EnableGridForExtraction(bool enable)
    {
        extractWhole = enable;
    }

    public void ActivateFire(bool firing)
    {
        Debug.Log("Firing");
        if (turnedOn && firing == true)
        {
            TurnOff();
        }
        else
        {
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
        extractWhole = false;
        mayExtract = false;
        fireActivating = false;
    }

    public override void TurnOff()
    {
        base.TurnOff();
        extractWhole = true;
        mayExtract = true;
        vfx.SendEvent("OnStop");
    }

    public override void OnHover(Vector2Int _onGridPosition)
    {
        base.OnHover(_onGridPosition);
        CheckSpoonCompatible();
        if (spoonActive)
            SetSpoon();
    }

    public override void OnExit()
    {
        base.OnExit();
        spoonActive = false;
        spoonSelection.SetActive(false);
    }

    public override void OnAction()
    {
        base.OnAction();
        if (spoonActive)
            HandleStirring();
    }

    public void CheckSpoonCompatible()
    {
        if(CookingHelperFunctions.GridCompatible(spoonOffsets, gridShape, onGridPosition))
        {
            if (spoonActive)
                return;
            spoonActive = true;
            spoonSelection.SetActive(true);
        }
        else
        {
            if (!spoonActive)
                return;
            spoonActive = false;
            spoonSelection.SetActive(false);
        }
    }

    public void SetSpoon()
    {
        Vector3 midPos = Vector3.Lerp(gridPositions[onGridPosition.x, onGridPosition.y].position, gridPositions[onGridPosition.x + 1, onGridPosition.y + 1].position, 0.5f);
        spoonSelection.transform.position = Vector3.Lerp(spoonSelection.transform.position, midPos, Time.deltaTime / 0.04f);
    }

    public void HandleStirring()
    {
        if (isStirring)
            return;
        StartCoroutine(Stir());
    }

    public IEnumerator Stir()
    {
        isStirring = true;
        List<FoodCell> selectedCells = new();

        foreach (Vector2Int offset in spoonOffsets)
        {
            Vector2Int gridPosition = onGridPosition + offset;
            if(gridOccupation[gridPosition.x, gridPosition.y] == 1)
            {
                foreach (FoodCell cell in cells)
                {
                    if (cell.gridPosition == gridPosition)
                    {
                        selectedCells.Add(cell);
                        cell.SetParent(spoonPivot, true);
                    }
                }
            }
        }

        float elapsedTime = 0f;
        Quaternion targetRotation = Quaternion.identity * Quaternion.Euler(0, 0, 90f);

        while (elapsedTime < stirDuration)
        {
            spoonPivot.localRotation = Quaternion.Lerp(Quaternion.identity, targetRotation, elapsedTime / stirDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spoonPivot.localRotation = Quaternion.identity;

        if (selectedCells.Count != 0)
            RotateSelection(selectedCells);

        isStirring = false;
    }

    public void RotateSelection(List<FoodCell> _selectedCells)
    {
        Vector2 tempOffset = CookingHelperFunctions.GetPreciseCenter(_selectedCells);
        foreach (FoodCell cell in _selectedCells)
        {
            Vector2Int newPos = CookingHelperFunctions.RotatePosition(cell.gridPosition, tempOffset, false);

            Vector2 worldPos = (newPos - tempOffset) * cellScale;
            cell.SetParent(cellHolder, true);
            cell.SetPosition(newPos, worldPos);
        }
        foreach (FoodCell cell in _selectedCells)
        {
            cell.UpdateVisual();
        }
    }
}

public struct CellRulePair
{
    public FoodCell cell;
    public CellTagRulePair rule;
}
