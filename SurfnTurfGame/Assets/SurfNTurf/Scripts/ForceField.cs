using UnityEngine;

public class ForceField : MonoBehaviour
{
    public float force = 10f;

    void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<WaterMovementController>() == null || !other.GetComponent<WaterMovementController>().enabled)
            return;
        other.attachedRigidbody?.AddForce(-transform.right * force, ForceMode.Force);
    }
}
