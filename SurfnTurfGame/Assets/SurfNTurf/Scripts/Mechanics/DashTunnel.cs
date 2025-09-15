using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DashTunnel : MonoBehaviour
{
    private void OnTriggerEnter(Collider collider)
    {
        if (collider.CompareTag("Player"))
        {
            MovementController playerMovement = collider.GetComponent<MovementController>();
            if (playerMovement.dv.dashing)
            {
                playerMovement.dv.dashLengthTimer = playerMovement.dv.dashLength;
            }
        }
    }
}
