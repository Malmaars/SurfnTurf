using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Gyser : MonoBehaviour
{
    public float power = 0.1f;
    private void Start()
    {
        StartCoroutine(GyserLife());
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.GetComponent<Rigidbody>().AddForce((Vector3.up  * power * other.GetComponent<Rigidbody>().linearVelocity.magnitude)+ Vector3.up, ForceMode.Impulse);
        }
    }
    private IEnumerator GyserLife()
    {
        while(true)
        {
            colliderToggle(false);
            yield return new WaitForSeconds(GetComponent<VisualEffect>().GetFloat("Anticipation"));
            colliderToggle(true);
            yield return new WaitForSeconds(GetComponent<VisualEffect>().GetFloat("Lifetime"));
            colliderToggle(false);
            yield return new WaitForSeconds(GetComponent<VisualEffect>().GetFloat("WaitingDelay"));
        }

        
    }

    private void colliderToggle(bool toggle)
    {
        foreach (Collider col in GetComponents<Collider>())
        {
            col.enabled = toggle;
        }
    }
}
