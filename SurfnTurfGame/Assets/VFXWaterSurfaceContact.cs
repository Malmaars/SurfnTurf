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
    private void VFXSpawn(GameObject other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (visualEffectAsset != visualEffect.visualEffectAsset)
            {
                visualEffect.visualEffectAsset = visualEffectAsset;
            }

            visualEffect.SetVector3("PlayerPosition", new Vector3(other.transform.position.x, waterLevel, other.transform.position.z));
            visualEffect.SendEvent("OnPlay");
        }
    }

    private void Update()
    {
        if (player.transform.position.y < waterLevel)
        {
            if (!submerged)
            {
                submerged = true;
                VFXSpawn(player);
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
