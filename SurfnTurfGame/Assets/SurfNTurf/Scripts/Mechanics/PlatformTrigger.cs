using System.Collections;
using UnityEngine;

public class PlatformTrigger : MonoBehaviour
{
    [SerializeField] private WalkingPlatform walkingPlatform;
    private Rigidbody playerRb;
    private Rigidbody playerRbHeld;
    private MovementController mov;
    private Vector3 delta;
    private bool isOnPlatform = false;
    [SerializeField] AnimationCurve decelerationCurve;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.GetComponent<Rigidbody>();
            playerRbHeld = playerRb;
            mov = other.GetComponent<MovementController>();
            isOnPlatform = true;
            //TODO dont grab the player but do it the right way
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isOnPlatform = false;
            StartCoroutine(AddDecayingVelocity(0.5f));
        }
    }
    private IEnumerator AddDecayingVelocity(float decayTime)
    {
        float elapsedTime = 0f;
        delta = walkingPlatform.GetPlatformDelta(transform);
        playerRb = null;

        while (elapsedTime < decayTime)
        {
            if (mov.gcv.grounded && elapsedTime > 0.1f) break;
            float decayFactor = 1f - (elapsedTime / decayTime);

            delta = walkingPlatform.GetPlatformDelta(transform)*decelerationCurve.Evaluate(decayFactor);
            elapsedTime += Time.deltaTime;
            playerRbHeld.MovePosition(playerRbHeld.position + delta);
            yield return null;
        }
;
    }

    private void LateUpdate()
    {
        if (playerRb != null && walkingPlatform != null)
        {
            if (isOnPlatform)
            {
                delta = walkingPlatform.GetPlatformDelta(transform);
            }
            playerRbHeld.MovePosition(playerRbHeld.position + delta);
        }
    }
}