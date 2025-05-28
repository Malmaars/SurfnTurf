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
    }
    
    public void StartChallenge()
    {
        if (currentChallenge == null)
            return;
        currentTime = currentChallenge.startTime;
        totalTime = 0;
        currentState = ChallengeStates.Running;
    }

    public void FailedChallenge()
    {
        if (currentChallenge == null)
            return;
        currentState = ChallengeStates.Failed;
        currentChallenge.StartFailedDirector();
    }

    public void CompleteChallenge()
    {
        if (currentChallenge == null)
            return;
        currentState = ChallengeStates.Presenting;
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
        if(currentTime <= 0)
        {
            if(BlackBoard.cookingManager != null) //change to bool for if there is food on the plate
            {
                FailedChallenge();
            }
            else
            {
                CompleteChallenge();
            }
        }
    }

    public void AddTime(float extraTime)
    {
        currentTime += extraTime;
    }
}
