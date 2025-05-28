using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurfnTurf;

public class ChallengeUI : MonoBehaviour
{
    [SerializeField] private TMP_Text Title;
    public TMP_Text timer;
    [SerializeField] private GameObject UI;
    [SerializeField] private TMP_Text[] objectives;
    [SerializeField] private Animator bars;
    [SerializeField] private Animator UIanimator;
    [SerializeField] AnimationCurve starCurve;
    [SerializeField] private GameObject[] stars;

    public Toggle[] objectivesChecks;

    public static ChallengeUI instance;

    private bool isOpen;

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

    public void OpenChallengeUI(String title, String[] challengeObjectives)
    {
        if (isOpen)
            return;
        for (int i = 0; i < objectives.Length; i++)
        {
            if (i < challengeObjectives.Length)
            {
                objectives[i].text = challengeObjectives[i]; // Set the objective text
                objectives[i].gameObject.SetActive(true); // Ensure the objective is visible
            }
            else
            {
                objectives[i].gameObject.SetActive(false); // Hide unused objective slots
            }
            stars[i].SetActive(false);
        }
        Title.text = title;
        bars.SetBool("Challenge",true);
        //UIanimator.SetBool("Challenge",true);
        isOpen = true;
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
