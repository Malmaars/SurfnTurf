using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.VFX;

public class WaveController : MonoBehaviour
{
    public float lifetime;
    public float size;
    private float sizeRandom;
    public float speed;
    public Transform collider;
    public VisualEffect vfx;
    private AnimationCurve curve;
    private UnityEngine.Vector3 originalLocation;

    private void Start()
    {
        originalLocation = transform.localPosition;
        StartCoroutine(ExecuteEvery(lifetime + 1f + Random.Range(0f, 5f)));
    }

    private void StartVFX()
    {
        sizeRandom = size + Random.Range(0.2f, 0.7f);
        transform.localPosition = originalLocation;
        vfx.SendEvent("OnPlay");
        vfx.SetFloat("Lifetime", lifetime);
        vfx.SetFloat("Size", sizeRandom);
        curve = vfx.GetAnimationCurve("AnimationCurve");
        StartCoroutine(EndAfterLifetime());
    }

    private IEnumerator EndAfterLifetime()
    {
        float elapsedTime = 0f;
        while (elapsedTime < lifetime)
        {
            transform.localPosition = originalLocation + transform.forward * speed * elapsedTime;
            collider.localScale = new UnityEngine.Vector3(sizeRandom, curve.Evaluate(elapsedTime / lifetime) * sizeRandom, sizeRandom);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        //gameObject.SetActive(false);
    }

    private IEnumerator ExecuteEvery(float seconds)
    {
        while (true)
        {
            yield return new WaitForSeconds(seconds);
            StartVFX();
        }
    }
}
