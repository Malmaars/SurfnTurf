using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    [SerializeField] private WalkingPlatform walkingPlatform;
    private Rigidbody playerRb;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.GetComponent<Rigidbody>();
            //TODO dont grab the player but do it the right way
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = null;
        }
    }

    private void LateUpdate()
    {
        if (playerRb != null && walkingPlatform != null)
        {
            Vector3 delta = walkingPlatform.GetPlatformDelta(transform);
            playerRb.MovePosition(playerRb.position + delta);
        }
    }
}