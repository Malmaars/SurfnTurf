using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderManager : MonoBehaviour
{
    ShaderKeyword useVertexSnapping;
    ShaderKeyword useWorldCurve;
    ShaderKeyword useFog; 
    ShaderKeyword useCoulds; 

    [Button("Toggle Vertex Snapping")]
    public void ToggleVertexSnapping()
    {
        useVertexSnapping = new ShaderKeyword("_USE_VERTEXSNAPPING");
        ToggleShaderEffect(useVertexSnapping);
    }
    [Button("Toggle WorldCurve")]
    public void ToggleWorldCurve()
    {
        useWorldCurve = new ShaderKeyword("_USE_WORLDCURVE");
        ToggleShaderEffect(useWorldCurve);
    }
    [Button("Toggle Fog")]
    public void ToggleFog()
    {
        useFog = new ShaderKeyword("_USE_FOG");
        ToggleShaderEffect(useFog);
    }
    [Button("Toggle Clouds")]
    public void ToggleClouds()
    {
        useCoulds = new ShaderKeyword("_USE_CLOUDS");
        ToggleShaderEffect(useCoulds);
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
