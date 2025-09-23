using UnityEngine;

public class LowGravityZoneTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MovementController mov = other.GetComponent<MovementController>();
            if (mov != null)
            {
                mov.lgz.isLowGravity = true;
            }
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            MovementController mov = other.GetComponent<MovementController>();
            if (mov != null)
            {
                mov.lgz.isLowGravity = false;
            }
        }
    }
}
