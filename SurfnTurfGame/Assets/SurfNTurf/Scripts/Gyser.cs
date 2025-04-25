using System.Collections;
using Unity.VisualScripting;
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
        float randomDelay = Random.Range(0f, 5f);
        StartCoroutine(GyserLife(randomDelay));
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
    private IEnumerator GyserLife(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            colliderToggle(false);
            transform.GetChild(0).transform.localScale = new Vector3(0f, 0f, 0f);
            yield return new WaitForSeconds(GetComponent<VisualEffect>().GetFloat("Anticipation"));
            colliderToggle(true);
            transform.GetChild(0).transform.localScale = new Vector3(1f, 1f, 1f);
            yield return new WaitForSeconds(GetComponent<VisualEffect>().GetFloat("Lifetime"));
            colliderToggle(false);
            transform.GetChild(0).transform.localScale = new Vector3(0f, 0f, 0f);
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

    private void Update()
    {
        ShaderManager shaderManager = ShaderManager.instance;
        transform.GetChild(0).gameObject.SetActive(!shaderManager.renderingVFX);
    }
}
