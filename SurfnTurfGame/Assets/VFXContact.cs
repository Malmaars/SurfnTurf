using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.VFX;

public class VFXContact : MonoBehaviour
{
    [SerializeField] private VisualEffectAsset visualEffectAsset;
    VisualEffect visualEffect;

    private void Start()
    {
        visualEffect = GetComponent<VisualEffect>();
    }
    private void VFXSpawn(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            if (visualEffectAsset != visualEffect.visualEffectAsset)
            {
                visualEffect.visualEffectAsset = visualEffectAsset;
            }

            visualEffect.SetVector3("PlayerPosition", other.transform.position);
            visualEffect.SendEvent("OnPlay");
        }
    }
    void OnTriggerEnter(Collider other)
    {
        VFXSpawn(other);
    }
    private void OnTriggerExit(Collider other)
    {
        //VFXSpawn(other);
    }
}
