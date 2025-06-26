using UnityEngine;

public class ChallengePresenter : MonoBehaviour
{
    public void EndChallenge()
    {
        if (BlackBoard.cookingManager.isDoingTutorial && BlackBoard.cookingManager.currentTutorialPart == 25)
        {
            BlackBoard.cookingManager.plate.ServeDish();
            BlackBoard.cookingManager.currentTutorialPart = 26;
            BlackBoard.cookingManager.TutorialNPC.ShowText(10, 0);
            BlackBoard.cookingManager.TutorialNPC.SwitchConversation(10);
        }
        else if(BlackBoard.cookingManager.isDoingTutorial){ return; }

        if (BlackBoard.challengeManager.currentState == ChallengeManager.ChallengeStates.Running)
        {
            if (BlackBoard.cookingManager.PutRemainingFoodOnPlate())
            {
                BlackBoard.challengeManager.CompleteChallenge();
            }
        }
    }
}
