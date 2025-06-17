using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurfnTurf;

public class ChallengeUI : MonoBehaviour
{
    public static ChallengeUI instance;

    public TMP_Text timer;
    [SerializeField] private Animator bars;
    [SerializeField] private GameObject[] stars;
    [SerializeField] private float timeBetweenStars;
    [SerializeField] private GameObject starContainer;
    [SerializeField] private GameObject criteriaContainer;
    [SerializeField] private GameObject exitButton;
    public Coroutine scoreScreen;

    [Header("ScoreContainer")]
    public Animator scoreCover;
    public TextMeshProUGUI scoreGoalText;
    public TextMeshProUGUI scoreReachedText;
    public ChallengeStamp scoreStamp;
    [Header("PreferencesContainer")]
    public Animator preferencesCover;
    public ChallengeStamp preferencesStamp;
    [Header("SpeedContainer")]
    public Animator speedCover;
    public TextMeshProUGUI speedGoalText;
    public TextMeshProUGUI speedReachedText;
    public ChallengeStamp speedStamp;



    private bool isOpen;

    //old
    Toggle[] objectivesChecks;
    AnimationCurve starCurve;
    private Animator UIanimator;
    private GameObject UI;
    private TMP_Text[] objectives;
    private TMP_Text Title;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instances
        }
    }

    public void OpenChallengeUI()
    {
        if (isOpen)
            return;
        timer.gameObject.SetActive(true);
        bars.SetBool("Challenge", true);
        //UIanimator.SetBool("Challenge", true);
        isOpen = true;
    }
    public void CloseChallengeUI()
    {
        if (!isOpen)
            return;
        timer.gameObject.SetActive(false);
        bars.SetBool("Challenge",false);
        //UIanimator.SetBool("Challenge",false);
        isOpen = false;
    }

    public void SetTimer(float time)
    {
        timer.text = time.SecondsToTime();
    }

    public void RunScoreScreen()
    {
        if (scoreScreen == null)
        {
            scoreScreen = StartCoroutine(ScoreScreen());
        }
        else
        {
            StopCoroutine(scoreScreen);
            scoreScreen = StartCoroutine(ScoreScreen());
        }
    }

    public IEnumerator ScoreScreen()
    {
        //reset everything
        foreach (GameObject star in stars)
        {
            star.SetActive(false);
        }
        starContainer.SetActive(true);
        criteriaContainer.SetActive(true);
        scoreCover.Play("CriteriaCover");
        preferencesCover.Play("CriteriaCover");
        speedCover.Play("CriteriaCover");
        scoreGoalText.text = "";
        scoreReachedText.text = "";
        speedGoalText.text = "";
        speedReachedText.text = "";
        scoreStamp.ResetStamp();
        preferencesStamp.ResetStamp();
        speedStamp.ResetStamp();

        bool[] criteria = new bool[] { BlackBoard.challengeManager.enoughScore, BlackBoard.challengeManager.satisfiedNPC, BlackBoard.challengeManager.inTime };

        for (int i = 0; i < criteria.Length; i++)
        {
            if(i == 0)
            {
                //open cover
                scoreCover.Play("CriteriaCoverOpen");
                yield return new WaitForSeconds(1f);

                //set score goal
                scoreGoalText.text = BlackBoard.challengeManager.currentChallenge.npc.scoreObjective.GetValueAmount().ToString();
                yield return new WaitForSeconds(1f);

                //show reached score
                float timer = 0;
                float timerDuration = 3f;
                int score = 0;
                int scoreToGoTo = BlackBoard.cookingManager.plate.GetTotalScore();
                while (timer < 1f)
                {
                    timer += Time.deltaTime / timerDuration;
                    scoreReachedText.text = ((int)Mathf.Lerp(score, scoreToGoTo, timer)).ToString();
                    yield return new WaitForEndOfFrame();
                }
                scoreReachedText.text = scoreToGoTo.ToString();

                //Animate stamp
                if (criteria[i])
                {
                    scoreStamp.PlaceStamp(true);
                    yield return new WaitForSeconds(1f);
                    stars[i].SetActive(true);
                }
                else
                {
                    scoreStamp.PlaceStamp(false);
                    yield return new WaitForSeconds(1f);
                }                
            }
            if(i == 1)
            {
                if (criteria[0])
                {
                    //open cover
                    preferencesCover.Play("CriteriaCoverOpen");
                    yield return new WaitForSeconds(1f);

                    //Animate stamp
                    if (criteria[i])
                    {
                        preferencesStamp.PlaceStamp(true);
                        yield return new WaitForSeconds(1f);
                        stars[i].SetActive(true);
                    }
                    else
                    {
                        preferencesStamp.PlaceStamp(false);
                        yield return new WaitForSeconds(1f);
                    }
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
            if (i == 2)
            {
                if (criteria[1])
                {
                    //open cover
                    speedCover.Play("CriteriaCoverOpen");
                    yield return new WaitForSeconds(1f);

                    //set score goal
                    speedGoalText.text = BlackBoard.challengeManager.currentChallenge.timeLeftCriteria.SecondsToTime();
                    yield return new WaitForSeconds(1f);

                    //show reached score
                    float timer = 0;
                    float timerDuration = 3f;
                    float score = 0;
                    float scoreToGoTo = BlackBoard.challengeManager.totalTime;
                    while (timer < 1f)
                    {
                        timer += Time.deltaTime / timerDuration;
                        speedReachedText.text = Mathf.Lerp(score, scoreToGoTo, timer).SecondsToTime();
                        yield return new WaitForEndOfFrame();
                    }
                    speedReachedText.text = scoreToGoTo.SecondsToTime() ;

                    //Animate stamp
                    if (criteria[i])
                    {
                        speedStamp.PlaceStamp(true);
                        yield return new WaitForSeconds(1f);
                        stars[i].SetActive(true);
                    }
                    else
                    {
                        speedStamp.PlaceStamp(false);
                        yield return new WaitForSeconds(1f);
                    }
                }
                else
                {
                    yield return new WaitForSeconds(1f);
                }
            }
            if (criteria[i])
            {
                if (BlackBoard.challengeManager.currentChallenge.toGiveCoins[i])
                {
                    BlackBoard.challengeManager.currentChallenge.GiveCoins(i);
                    BlackBoard.challengeManager.currentChallenge.toGiveCoins[i] = false;
                }
                yield return new WaitForSeconds(timeBetweenStars);
            }
        }

        exitButton.SetActive(true);

        yield return null;
    }

    public void CloseScoreScreen()
    {
        starContainer.SetActive(false);
        criteriaContainer.SetActive(false);
        exitButton.SetActive(false);
    }

    
    public IEnumerator EndingSequance(Challenge challenge)
    {
        /*
        bars.SetBool("Challenge",false);
        for(int i = 0; i < stars.Length; i++)
        {
            if(!objectivesChecks[i].isOn) continue;
            stars[i].SetActive(true);
            float elapsedTime = 0f;
            Vector3 startScale = stars[i].transform.localScale;
            while (elapsedTime < 0.5)
            {
                elapsedTime += Time.deltaTime;
                stars[i].transform.localScale = startScale + (new Vector3(1,1,1) * starCurve.Evaluate(elapsedTime/0.5f));
                yield return null;
            }
            stars[0].transform.localScale = startScale;
        }
        CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challenge.challengeName).hasReachedEnd = objectivesChecks[0].isOn;
        CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challenge.challengeName).hasNotUsedRestrictions = objectivesChecks[1].isOn;
        CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challenge.challengeName).hasReachedendWithinTime = objectivesChecks[2].isOn;
        if (CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challenge.challengeName).time > challenge.currentTime)
            CloudSaveSystem.Instance.data.challengeDatas.Find(item => item.key == challenge.challengeName).time = challenge.currentTime;
        challenge.UpdateStars();
        CloseChallengeUI();
        */
        yield return new WaitForSeconds(3f);
    }
    
}
