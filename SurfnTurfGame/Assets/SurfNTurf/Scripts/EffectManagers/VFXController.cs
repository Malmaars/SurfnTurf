using UnityEditor.Search;
using UnityEngine;
using UnityEngine.VFX;

[RequireComponent(typeof(VisualEffect))]
public class VFXController : MonoBehaviour
{
    [SerializeField] private VisualEffect visualEffect;
    public void PlayVisualEffect(Object Object)
    {
        VisualEffectAsset vfxAsset = Object as VisualEffectAsset;
        if (vfxAsset != null)
        {
            if(visualEffect != visualEffect.visualEffectAsset)
                visualEffect.visualEffectAsset = vfxAsset;

            
            visualEffect.SendEvent("OnPlay");
        }
        else
        {
            Debug.LogError("The provided asset is not a VisualEffectAsset.");
        }
    }
}
