using UnityEngine;

public class CookingSpot : MonoBehaviour
{
    public Transform cookingLocation;
    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            BlackBoard.cookingManager.SetCookingLocation(cookingLocation);
            BlackBoard.cookingManager.onCookingLocation = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.tag == "Player")
        {
            BlackBoard.cookingManager.onCookingLocation = false;
        }
    }
}
