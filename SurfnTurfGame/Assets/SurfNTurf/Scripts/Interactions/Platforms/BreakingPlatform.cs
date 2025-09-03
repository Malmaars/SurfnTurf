using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BreakingPlatform : MonoBehaviour
{
    [Header("Platform State")]
    public BreakablePlatformStates platformState;
    public enum BreakablePlatformStates { Stationary, Loaded, Broken }
    [Header("Platform Settings")]
    public float collapseTime;
    public float respawnTime;
    private float collapseTimeLeft;
    private float respawnTimeLeft;

    public Material stationaryColor;
    public Material breakingColor;
    public Material brokenColor;
    private Collider platformCollider;
    private MeshRenderer platformMesh;

    private void Start()
    {
        platformCollider = GetComponent<Collider>();
        platformMesh = GetComponent<MeshRenderer>();

        platformMesh.material = stationaryColor;

        platformCollider.enabled = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag != "Player")
            return;

        if(platformState == BreakablePlatformStates.Stationary)
            StartBreaking();
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag != "Player")
            return;
        if (platformState == BreakablePlatformStates.Loaded)
            BreakPlatform();
    }

    private void Update()
    {
        switch (platformState)
        {
            case BreakablePlatformStates.Stationary:
                break;
            case BreakablePlatformStates.Loaded:
                collapseTimeLeft -= Time.deltaTime;
                if (collapseTimeLeft <= 0)
                    BreakPlatform();
                break;
            case BreakablePlatformStates.Broken:
                respawnTimeLeft -= Time.deltaTime;
                if (respawnTimeLeft <= 0)
                    RespawnPlatform();
                break;
            default:
                break;
        }
    }

    private void StartBreaking()
    {
        platformState = BreakablePlatformStates.Loaded;
        collapseTimeLeft = collapseTime;

        platformMesh.material = breakingColor;
    }

    private void BreakPlatform()
    {
        platformState = BreakablePlatformStates.Broken;
        respawnTimeLeft = respawnTime;

        platformMesh.material = brokenColor;

        platformCollider.enabled = false;
    }

    private void RespawnPlatform()
    {
        platformState = BreakablePlatformStates.Stationary;

        platformMesh.material = stationaryColor;

        platformCollider.enabled = true;
    }
}
