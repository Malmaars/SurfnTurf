using UnityEngine;
using UnityEngine.Playables;

public class StartPlayableOnTrigger : MonoBehaviour
{
    [SerializeField] private PlayableDirector playableDirector; // Reference to the PlayableDirector

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering the trigger is the player or any other specific object
        if (other.CompareTag("Player"))
        {
            // Start the PlayableDirector
            playableDirector.Play();
        }
    }
}