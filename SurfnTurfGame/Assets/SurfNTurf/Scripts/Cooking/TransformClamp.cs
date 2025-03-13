using UnityEngine;
using UnityEngine.Rendering;

public class TransformClamp : MonoBehaviour
{
    public Transform target;
    public bool clampPosition;
    public bool clampRotation;

    private void OnEnable()
    {
        RenderPipelineManager.beginCameraRendering += Test;
    }

    private void OnDisable()
    {
        RenderPipelineManager.beginCameraRendering -= Test;
    }

    private void Test(ScriptableRenderContext context, Camera camera)
    {
        if (clampPosition)
            transform.position = target.position;
        if (clampRotation)
            transform.rotation = target.rotation;
    }
}
