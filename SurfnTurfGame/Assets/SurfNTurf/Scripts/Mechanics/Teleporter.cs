using NaughtyAttributes;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Transform targetLocation;
    private bool wasStandingOnTeleportWhileOnCooldown = false;
    private MovementController mov;
    [ReadOnly]private bool isActive = true;
    private void Start()
    {
        mov = BlackBoard.playerBody.GetComponent<MovementController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (isActive)
            {
                isActive = false;
                Invoke(nameof(ResetActive), mov.tv.teleportCooldown);
                mov.GetComponent<Rigidbody>().MovePosition(targetLocation.position);
            }
            else
            {
                wasStandingOnTeleportWhileOnCooldown = true;
            }
        }
    }

    private void ResetActive()
    {
        if (wasStandingOnTeleportWhileOnCooldown)
        {
            wasStandingOnTeleportWhileOnCooldown = false;
            Invoke(nameof(ResetActive), mov.tv.teleportCooldown);
        }
        else
        {
            isActive = true;
        }
    }
}
