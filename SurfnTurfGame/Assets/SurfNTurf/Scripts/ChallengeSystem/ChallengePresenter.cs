using UnityEngine;

public class ChallengePresenter : MonoBehaviour
{
    public void EndChallenge()
    {
        if(BlackBoard.challengeManager.currentState == ChallengeManager.ChallengeStates.Running)
        {
            if (BlackBoard.cookingManager.PutRemainingFoodOnPlate())
            {
                BlackBoard.challengeManager.CompleteChallenge();
            }
        }
    }
}
