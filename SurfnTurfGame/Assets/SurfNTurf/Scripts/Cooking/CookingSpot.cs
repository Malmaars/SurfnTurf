using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class CookingSpot : MonoBehaviour
{
    public Transform cookingLocation;
    public GameObject cookingSpotVisual;
    public GameObject[] stars;
    public TMP_Text time;
    private void Start()
    {
        UIManager.instance.compass.AddWaypoint(transform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            BlackBoard.cookingManager.SetCookingLocation(this);
            BlackBoard.cookingManager.onCookingLocation = true;
            if (BlackBoard.challengeManager.currentState != ChallengeManager.ChallengeStates.Inactive || BlackBoard.playerManager.GetCurrentState() == BlackBoard.cookingManager)
            {
                cookingSpotVisual.SetActive(false);

            }
            else
            {
                cookingSpotVisual.SetActive(true);

            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            if (cookingSpotVisual.activeSelf)
            {
                if (BlackBoard.challengeManager.currentState != ChallengeManager.ChallengeStates.Inactive || BlackBoard.playerManager.GetCurrentState() == BlackBoard.cookingManager)
                {
                    cookingSpotVisual.SetActive(false);
                }
            }
            else
            {
                if (BlackBoard.playerManager.GetCurrentState() != BlackBoard.cookingManager)
                {
                    cookingSpotVisual.SetActive(true);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            BlackBoard.cookingManager.onCookingLocation = false;
            cookingSpotVisual.SetActive(false);
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
