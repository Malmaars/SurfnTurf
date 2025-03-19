using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.VFX;

public class VFXWaterSurfaceContact : MonoBehaviour
{
    [SerializeField] private VisualEffectAsset visualEffectAsset;
    VisualEffect visualEffect;
    [SerializeField] private GameObject player;
    private bool submerged = false;
    public float waterLevel = 0.0f;

    private void Start()
    {
        visualEffect = GetComponent<VisualEffect>();
    }
    private void VFXSpawn()
    {
        if (visualEffectAsset != visualEffect.visualEffectAsset)
        {
            visualEffect.visualEffectAsset = visualEffectAsset;
        }

        visualEffect.SetVector3("PlayerPosition", new Vector3(player.transform.position.x, waterLevel - 1.5f, player.transform.position.z));
        visualEffect.SendEvent("OnPlay");
    }
    public void Splash()
    {
        if (visualEffectAsset != visualEffect.visualEffectAsset)
        {
            visualEffect.visualEffectAsset = visualEffectAsset;
        }

        visualEffect.SetVector3("PlayerPosition", player.transform.position);
        visualEffect.SendEvent("OnPlay");
    }

    private void Update()
    {
        if (player.transform.position.y < waterLevel)
        {
            if (!submerged)
            {
                submerged = true;
                VFXSpawn();
            }
        }
        else
        {
            if (submerged)
            {
                submerged = false;
            }
        }
    }
}
