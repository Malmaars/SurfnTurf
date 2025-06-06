using UnityEngine;

public class CookingSpot : MonoBehaviour
{
    public Transform cookingLocation;
    public GameObject cookingSpotEffect;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            BlackBoard.cookingManager.SetCookingLocation(this);
            BlackBoard.cookingManager.onCookingLocation = true;
            if (BlackBoard.challengeManager.currentState != ChallengeManager.ChallengeStates.Inactive)
            {
                cookingSpotEffect.SetActive(false);

            }
            else
            {
                cookingSpotEffect.SetActive(true);

            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            BlackBoard.cookingManager.onCookingLocation = false;
            cookingSpotEffect.SetActive(false);
        }
    }

    //drag gizmo to set the cooking location
    private void OnDrawGizmos()
    {
        if (cookingLocation != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(cookingLocation.position, 0.5f);
            Gizmos.DrawLine(transform.position, cookingLocation.position);
        }
    }
}
