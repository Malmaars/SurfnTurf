using UnityEngine;

public class RatingTrigger : MonoBehaviour
{
    public int index = 0;
    private void OnTriggerEnter(Collider other)
    {
        if (BlackBoard.playerManager.GetCurrentState().GetType() == typeof(MainMenuState)) return;
        
        if (other.CompareTag("Player"))
        {
            UIManager.instance.ratingSystem.OpenRating(index);
            gameObject.SetActive(false);
        }
    }
}
