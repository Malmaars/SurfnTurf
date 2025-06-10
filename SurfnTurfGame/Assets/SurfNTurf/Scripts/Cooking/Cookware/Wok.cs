using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using FMODUnity;
using UnityEngine.VFX;
using FMOD.Studio;

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
    public int tutorialCount;

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
    public EventReference scoreUpReference;
    public string scoreUpParameterReference;
    public EventReference scoreDownReference;
    public string scoreDownParameterReference;

    public override void ActivateGrid(float _cellScale)
    {
        base.ActivateGrid(_cellScale);
        spoonVisual.localScale = new Vector3(cellScale * 2, cellScale * 2, 1f);
    }

    public override void SetCells(List<FoodCell> _cells, Vector2Int _onGridPosition)
    {
        base.SetCells(_cells, _onGridPosition);

        if (BlackBoard.cookingManager.isDoingTutorial && BlackBoard.cookingManager.currentTutorialPart == 1)
        {
            tutorialCount++;
            if(tutorialCount >= 2)
            {
                BlackBoard.cookingManager.currentTutorialPart = 2;
                BlackBoard.cookingManager.AddSpoon(0);
                BlackBoard.cookingManager.AddSpoon(2);
                BlackBoard.cookingManager.AddSpoon(1);
                UIManager.instance.tutorialPart = TutorialUIPart.Tools;
                UIManager.instance.ShowTutorial(true);
            }
        }

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
            animatingScore = StartCoroutine(AnimateSettingScore(scoredCells));
        }
        else
        {
            animatingScore = StartCoroutine(AnimateSettingScore(scoredCells));
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

    IEnumerator AnimateSettingScore(List<FoodCell> _cells)
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
                    foreach (TagRule rule in cell.cellScore.mainTag.rules)
                    {
                        if (rule.DoesHaveRuleInteraction(neighbourCell))
                        {
                            CellRulePair cellRulePair = new() {cell = cell, rule = rule, neighbourCell = neighbourCell};
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
                cellRulePair.cell.PlayScoreAnimation(cellRulePair.neighbourCell.cellScore.mainTag, timeBetweenScore*2);
                int score = cellRulePair.rule.Calculate(new List<FoodCell> { cellRulePair.neighbourCell }, cellRulePair.cell);

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
            myEvent = FMODUnity.RuntimeManager.CreateInstance(scoreUpReference);
            myEvent.setParameterByName(scoreUpParameterReference, audioModifier); //go to 1 for every scoring
            myEvent.start();
            myEvent.release();

        }
        else if (score < 0)
        {
            StartCoroutine(AnimateScore(false, timeToAnimate, modifier));
            FMOD.Studio.EventInstance myEvent;
            myEvent = FMODUnity.RuntimeManager.CreateInstance(scoreDownReference);
            myEvent.setParameterByName(scoreDownParameterReference, audioModifier); //go to 1 for every scoring
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
        if (spoonActive && BlackBoard.cookingManager.spoonsLeft)
            HandleStirring();
    }

    public void CheckSpoonCompatible()
    {
        if(CookingHelperFunctions.GridCompatible(spoonOffsets, gridShape, onGridPosition) && turnedOn)
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
        if (isStirring)
            return;
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

        Vector2Int clickedPosition = onGridPosition;

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
            RotateSelection(selectedCells, clickedPosition);

        BlackBoard.cookingManager.UseSpoon();

        isStirring = false;
    }

    public void RotateSelection(List<FoodCell> _selectedCells, Vector2Int _clickedPosition)
    {
        List<Vector2Int> newPositions = CookingHelperFunctions.RotatePoints(_selectedCells, _clickedPosition, new Vector2Int(2,2), false);
        for (int i = 0; i < _selectedCells.Count; i++)
        {
            gridOccupation[_selectedCells[i].gridPosition.x, _selectedCells[i].gridPosition.y] = 0;
        }
        for (int i = 0; i < _selectedCells.Count; i++)
        {
            _selectedCells[i].SetParent(cellHolder, true);
            _selectedCells[i].SetPosition(newPositions[i], gridPositions[newPositions[i].x, newPositions[i].y].localPosition);
            gridOccupation[newPositions[i].x, newPositions[i].y] = 1;
        }

        foreach (FoodCell cell in cells)
        {
            cell.SetNeighbors(cells);
            cell.UpdateVisual();
        }

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
}

public struct CellRulePair
{
    public FoodCell cell;
    public TagRule rule;
    public FoodCell neighbourCell;
}
