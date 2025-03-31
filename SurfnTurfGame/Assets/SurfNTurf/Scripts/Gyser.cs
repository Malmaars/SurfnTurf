using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class Gyser : MonoBehaviour
{
    public float pulse = 20f;
    public float initialPulse = 30f;
    public float ImpulseCooldown = 4f;

    private bool canDoHardImpusle = true;

    private IEnumerator HardImpulse()
    {
        canDoHardImpusle = false;
        yield return new WaitForSeconds(ImpulseCooldown);
        canDoHardImpusle = true;
    }
    private void Start()
    {
        StartCoroutine(GyserLife());
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (canDoHardImpusle)
            {
                StartCoroutine(HardImpulse());
                other.GetComponent<Rigidbody>().AddForce(Vector3.up * initialPulse, ForceMode.Impulse);
            }
            else
            {
                other.GetComponent<Rigidbody>().AddForce(Vector3.up * pulse, ForceMode.Impulse);
            }
        }
    }
    private IEnumerator GyserLife()
    {
        while (true)
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
