using UnityEngine;

public class Whirlpool : MonoBehaviour
{
    public float forceStrength = 100f; // Strength of the whirlpool force
    private void OnTriggerStay(Collider other)
    {
        //add a force towards the center of the whirlpool
        if (other.CompareTag("Player"))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                Vector3 direction = transform.position - other.transform.position;
                rb.AddForce(direction.normalized * forceStrength, ForceMode.Acceleration);
            }
        }
    }
}
