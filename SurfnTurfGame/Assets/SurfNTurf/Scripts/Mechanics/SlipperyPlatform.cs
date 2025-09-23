using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SlipperyPlatform : MonoBehaviour
{
    private MovementController playerMovement;
    private WaterMovementController playerWaterMovement;
    private bool colliding;
    private float previousAcceleration;
    private float previousMaxSpeed;

    private void Start()
    {
        playerMovement = BlackBoard.playerBody.GetComponent<MovementController>();
        playerWaterMovement = BlackBoard.playerBody.GetComponent<WaterMovementController>();
    }

    private void OnCollisionStay(Collision collision)
    {
        if (colliding)
            return;

        if (collision.gameObject.CompareTag("Player") && playerMovement.gcv.grounded)
        {
            previousAcceleration = playerMovement.gcv.maxAcceleration;
            previousMaxSpeed = playerMovement.gcv.maxSpeed;
            playerMovement.gcv.maxAcceleration = 10;
            playerMovement.gcv.maxSpeed = playerMovement.gcv.maxSpeed * 1.5f;
            colliding = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!colliding)
            return;

        if (collision.gameObject.CompareTag("Player"))
        {
            playerMovement.gcv.maxAcceleration = previousAcceleration;
            playerMovement.gcv.maxSpeed = previousMaxSpeed;
            colliding = false;
        }
    }
}
