using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BobShadowManager : MonoBehaviour
{
    public DecalProjector decal;
    public Transform trackingTarget;
    public PlayerManager playerManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = trackingTarget.position - Vector3.up * 0.5f;
        transform.rotation = Quaternion.Euler(90, 0, 0);

        if (playerManager.GetCurrentState().GetType() == typeof(MovementController))
            decal.enabled = true;
        else
            decal.enabled = false;
    }
}
