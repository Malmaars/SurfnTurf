using UnityEngine;
using UnityEngine.VFX;

[ExecuteInEditMode]
public class MyTimeProvider : MonoBehaviour
{
    [SerializeField] private Renderer[] renderers;
    [SerializeField] private VisualEffect[] vfxs;
    void Update()
    {
        if (renderers == null || vfxs == null)
        {
            return;
        }
        foreach (var renderer in renderers)
        {
            renderer.material.SetFloat("_MyTime", Time.time);
        }
        foreach (var vfx in vfxs)
        {
            vfx.SetFloat("_MyTime", Time.time);
        }
    }
}
