using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SequencedPlatform : MonoBehaviour
{
    private enum SequencedPlatformState {Idle, Primed, Active}
    private SequencedPlatformState platformState;

    [Header("Platform Settings")]
    public float primeTime;
    private float primeTimer;
    public float activeTime;
    private float activeTimer;

    public Material idleMaterial;
    public Material activeMaterial;

    private void Update()
    {
        switch (platformState)
        {
            case SequencedPlatformState.Idle:
                break;
            case SequencedPlatformState.Primed:
                primeTimer -= Time.deltaTime;
                if (primeTimer <= 0)
                {
                    Activate();
                }
                break;
            case SequencedPlatformState.Active:
                activeTimer -= Time.deltaTime;
                if (activeTimer <= 0)
                {
                    Deactivate();
                }
                break;
            default:
                break;
        }
    }

    public void Prime()
    {
        primeTimer = primeTime;
        platformState = SequencedPlatformState.Primed;
    }

    public void Activate()
    {
        activeTimer = activeTime;
        platformState = SequencedPlatformState.Active;

        GetComponent<Collider>().enabled = true;
        GetComponent<MeshRenderer>().material = activeMaterial;
    }

    public void Deactivate()
    {
        platformState = SequencedPlatformState.Idle;

        GetComponent<Collider>().enabled = false;
        GetComponent<MeshRenderer>().material = idleMaterial;
    }
}
