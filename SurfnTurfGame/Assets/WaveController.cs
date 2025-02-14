using System.Collections;
using System.Numerics;
using UnityEngine;
using UnityEngine.VFX;

public class WaveController : MonoBehaviour
{
    public float lifetime;
    public float size;
    public float speed;
    public Transform collider;
    public VisualEffect vfx;
    private AnimationCurve curve;
    private UnityEngine.Vector3 originalLocation;

    private void Start()
    {
        originalLocation = transform.position;
        StartCoroutine(ExecuteEvery(lifetime+1f+Random.Range(0f,5f)));
    }

    private void StartVFX()
    {
        transform.position = originalLocation;
        vfx.SendEvent("OnPlay");
        vfx.SetFloat("Lifetime", lifetime);
        vfx.SetFloat("Size", size);
        curve = vfx.GetAnimationCurve("AnimationCurve");
        StartCoroutine(EndAfterLifetime());
    }

    private IEnumerator EndAfterLifetime()
    {
        float elapsedTime = 0f;
        while (elapsedTime < lifetime)
        {
            transform.position = new UnityEngine.Vector3(originalLocation.x, transform.position.y, originalLocation.z+ speed * elapsedTime);
            collider.localScale = new UnityEngine.Vector3(size, curve.Evaluate(elapsedTime / lifetime)*size,size);
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
