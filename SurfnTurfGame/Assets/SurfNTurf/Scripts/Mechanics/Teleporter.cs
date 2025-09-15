using NaughtyAttributes;
using UnityEngine;

public class Teleporter : MonoBehaviour
{
    [SerializeField] private Transform targetLocation;
    private bool wasStandingOnTeleportWhileOnCooldown = false;
    private MovementController mov;
    private void Start()
    {
        mov = BlackBoard.playerBody.GetComponent<MovementController>();
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            if (mov.tv.activeTeleporters)
            {
                mov.tv.activeTeleporters = false;
                Invoke(nameof(ResetActive), mov.tv.teleportCooldown);
                mov.GetComponent<Rigidbody>().MovePosition(targetLocation.position);
            }
            else
            {
                wasStandingOnTeleportWhileOnCooldown = true;
            }
        }
    }

    public void ResetActive()
    {
        if (wasStandingOnTeleportWhileOnCooldown)
        {
            wasStandingOnTeleportWhileOnCooldown = false;
            Invoke(nameof(ResetActive), mov.tv.teleportCooldown);
        }
        else
        {
            mov.tv.activeTeleporters = true;
        }
    }
}
