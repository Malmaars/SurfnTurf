using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderManager : MonoBehaviour
{
    ShaderKeyword useVertexSnapping;
    ShaderKeyword useWorldCurve;

    [Button("Toggle Shader Vertex Snapping")]
    public void ToggleVertexSnapping()
    {
        useVertexSnapping = new ShaderKeyword("_USE_VERTEXSNAPPING");
        ToggleShaderEffect(useVertexSnapping);
    }
    [Button("Toggle Shader WorldCurve")]
    public void ToggleWorldCurve()
    {
        useWorldCurve = new ShaderKeyword("_USE_WORLDCURVE");
        ToggleShaderEffect(useWorldCurve);
    }
    public void ToggleShaderEffect(ShaderKeyword keyword)
    {
        if (Shader.IsKeywordEnabled(keyword.name))
        {
            Shader.DisableKeyword(keyword.name);
        }
        else
        {
            Shader.EnableKeyword(keyword.name);
        }
    }


}
