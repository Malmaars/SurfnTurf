using NaughtyAttributes;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
    public enum ChallengeStates { Inactive, Initiation, Running, Failed, Presenting}
    public ChallengeStates currentState;
    public NewChallenge currentChallenge;
    public bool startChallenge;

    [Header("Current Challenge Data")]
    public float currentTime;
    public float totalTime;

    private void Awake()
    {
        BlackBoard.challengeManager = this;
        currentState = ChallengeStates.Inactive;
    }

    private void Update()
    {
        if (startChallenge && currentState == ChallengeStates.Inactive)
        {
            startChallenge = false;
            InitializeChallenge();
        }
        if(currentState == ChallengeStates.Running)
        {
            CountDownRunningTime();
        }
    }

    //Challenge Functions
    public void RetrieveChallenge(NewChallenge challenge)
    {
        currentChallenge = challenge;
    }
    public void DiscardChallenge(NewChallenge challenge)
    {
        if(currentState != ChallengeStates.Inactive)
        {
            QuitChallenge();
        }
        currentChallenge = null;
    }
    
    [Button("Start Challenge")]
    public void InitializeChallenge()
    {
        if (currentChallenge == null)
            return;
        currentChallenge.StartInitiationDirector();
        currentState = ChallengeStates.Initiation;
        MusicManager.instance.StopChallengeMusic();
    }

    public void StartChallenge()
    {
        if (currentChallenge == null)
            return;
        currentTime = currentChallenge.startTime;
        totalTime = 0;
        ChallengeUI.instance.OpenChallengeUI();
        currentState = ChallengeStates.Running;
    }

    public void FailedChallenge()
    {
        if (currentChallenge == null)
            return;

        ChallengeUI.instance.CloseChallengeUI();

        currentState = ChallengeStates.Failed;
        MusicManager.instance.StopChallengeMusic();
        currentChallenge.StartFailedDirector();
    }

    public void CompleteChallenge()
    {
        if (currentChallenge == null)
            return;

        ChallengeUI.instance.CloseChallengeUI();

        currentState = ChallengeStates.Presenting;
        MusicManager.instance.StopChallengeMusic();
        currentChallenge.StartPresentingDirector();
    }

    public void QuitChallenge()
    {
        //switch player to movement state
        //completely exit this challenge
        currentState = ChallengeStates.Inactive;
    }

    //Time Functions
    public void CountDownRunningTime()
    {
        currentTime -= Time.deltaTime;
        totalTime += Time.deltaTime;
        ChallengeUI.instance.SetTimer(currentTime);
        if(currentTime <= 0)
        {
            
            if(BlackBoard.cookingManager.PutRemainingFoodOnPlate()) //change to bool for if there is food on the plate
            {
                CompleteChallenge();
            }
            else
            {
                FailedChallenge();
                ChallengeUI.instance.SetTimer(0);
            }
        }
    }

    public void AddTime(float extraTime)
    {
        currentTime += extraTime;
    }
}
