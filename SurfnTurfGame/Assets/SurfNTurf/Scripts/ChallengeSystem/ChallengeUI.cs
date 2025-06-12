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
    public Coroutine scoreScreen;



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
        foreach (GameObject star in stars)
        {
            star.SetActive(false);
        }
        starContainer.SetActive(true);

        bool[] criteria = new bool[] { BlackBoard.challengeManager.inTime, BlackBoard.challengeManager.enoughScore, BlackBoard.challengeManager.satisfiedNPC };

        for (int i = 0; i < criteria.Length; i++)
        {
            if (criteria[i])
            {
                stars[i].SetActive(true);
                yield return new WaitForSeconds(timeBetweenStars);
            }
        }
        yield return null;
    }

    
    public IEnumerator EndingSequance(Challenge challenge)
    {
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
        yield return new WaitForSeconds(3f);
        CloseChallengeUI();
    }
    
}
