using UnityEngine;

[RequireComponent(typeof(Collider))]
public class StickyPlatform : MonoBehaviour
{
    public Restriction restriction;
    private MovementController playerMovement;
    private WaterMovementController playerWaterMovement;
    private bool colliding;
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
            restriction.ApplyRestriction(restriction, playerMovement, playerWaterMovement);
            previousMaxSpeed = playerMovement.gcv.maxSpeed;
            playerMovement.gcv.maxSpeed = playerMovement.gcv.maxSpeed / 2;
            colliding = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (!colliding)
            return;
        
        if (collision.gameObject.CompareTag("Player"))
        {
            restriction.UndoRestriction(restriction, playerMovement, playerWaterMovement);
            playerMovement.gcv.maxSpeed = previousMaxSpeed;
            colliding = false;
        }
    }
}
