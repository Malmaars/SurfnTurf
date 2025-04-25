using System.Collections;
using UnityEngine;
using UnityEngine.VFX;

public class WaveController : MonoBehaviour
{
    public float lifetime;
    public float size;
    private float sizeRandom;
    public float speed;
    public Transform col;
    public VisualEffect vfx;
    private AnimationCurve curve;
    private Vector3 originalLocation;
    private Vector3 originalGizmoLocation;
    [SerializeField]private GameObject fallBack;

    private void Start()
    {
        originalLocation = transform.localPosition;
        originalGizmoLocation = transform.position;
        float randomDelay = Random.Range(0f, 5f);
        StartCoroutine(ExecuteEvery(lifetime + 1f + randomDelay));
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
            col.localScale = new Vector3(sizeRandom, curve.Evaluate(elapsedTime / lifetime) * sizeRandom, sizeRandom);
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;

        // Ensure originalLocation is set
        if (originalGizmoLocation == Vector3.zero)
        {
            originalGizmoLocation = transform.position;
        }

        Vector3 startPosition = originalGizmoLocation;
        Vector3 endPosition = originalGizmoLocation + transform.forward * speed * lifetime *1f;
        Gizmos.DrawLine(startPosition, endPosition);
    }

    private void Update()
    {
        ShaderManager shaderManager = ShaderManager.instance;
        fallBack.SetActive(!shaderManager.renderingVFX);
    }
}
