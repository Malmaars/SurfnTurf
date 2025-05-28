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
