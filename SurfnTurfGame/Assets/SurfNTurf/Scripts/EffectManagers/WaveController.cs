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
    [SerializeField] private GameObject fallBack;
    [SerializeField] private GameObject colliderTrigger;
    [SerializeField] private Transform playerVisuals;
    [SerializeField] public Transform surfLocation;
    public float lerpSpeed = 10f;
    private Rigidbody rb;
    public WaterMovementController mov;
    
    public bool playerOnWave = false;
    private bool waveInProggress = false;
    float elapsedTime = 0f;

    private void Start()
    {
        originalLocation = transform.localPosition;
        originalGizmoLocation = transform.position;
        colliderTrigger.SetActive(false);
        rb = BlackBoard.playerBody.GetComponent<Rigidbody>();
        playerVisuals = BlackBoard.playerBody.GetComponent<MovementController>().playerVisual;
        mov = BlackBoard.playerBody.GetComponent<WaterMovementController>();
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
        elapsedTime = 0f;
        waveInProggress = true;
    }
    public void ResetPlayerVelocity()
    {
        colliderTrigger.SetActive(false);
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        mov.suv.onWave = true;
        lerpSpeed = 1f;
    }

    private void EndAfterLifetime()
    {
        if (!waveInProggress) return;

        if (elapsedTime < lifetime)
        {
            if (elapsedTime > lifetime * 0.8f)
            {
                colliderTrigger.SetActive(false);
                if (playerOnWave)
                {
                    rb.isKinematic = false;
                    rb.linearVelocity = transform.forward * speed * 1.1f;
                    colliderTrigger.SetActive(false);
                }
                playerOnWave = false;
            }
            else if (elapsedTime > lifetime * 0.1f)
            {
                if (!playerOnWave)
                {
                    colliderTrigger.SetActive(true);
                }
            }
                transform.localPosition = originalLocation + transform.forward * speed * elapsedTime;
                col.localScale = new Vector3(sizeRandom, curve.Evaluate(elapsedTime / lifetime) * sizeRandom, sizeRandom);
                elapsedTime += Time.deltaTime;
            }
            if (elapsedTime >= lifetime)
            {
                mov.suv.onWave = false;
                waveInProggress = false;
            }
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
        Vector3 endPosition = originalGizmoLocation + transform.forward * speed * lifetime * 1f;
        Gizmos.DrawLine(startPosition, endPosition);
    }

    private void LateUpdate()
    {
        ShaderManager shaderManager = ShaderManager.instance;
        fallBack.SetActive(!shaderManager.renderingVFX);
        EndAfterLifetime();
/*        if (playerOnWave)
        {
            rb.MovePosition(surfLocation.position);
            //rb.MovePosition(Vector3.Lerp(rb.transform.position, surfLocation.position, Time.deltaTime * lerpSpeed));
            lerpSpeed = lerpSpeed + Time.deltaTime * 50f;
            playerVisuals.forward = surfLocation.forward;
        }*/
    }


}
