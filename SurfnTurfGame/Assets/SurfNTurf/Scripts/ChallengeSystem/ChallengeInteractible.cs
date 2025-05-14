using UnityEngine;

public class ChallengeInteractible : Interactible
{
    [SerializeField] Challenge challenge;
    public override bool InteractWith()
    {
        challenge.StartChallenge();
        return false;
    }
    
}
