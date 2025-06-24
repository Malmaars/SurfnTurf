using UnityEngine;

public class ForceField : MonoBehaviour
{
    public float force = 10f;

    void OnTriggerStay(Collider other)
    {
        other.attachedRigidbody?.AddForce(-transform.right * force, ForceMode.Force);
    }
}
