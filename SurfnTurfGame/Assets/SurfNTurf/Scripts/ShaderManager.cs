using NaughtyAttributes;
using UnityEngine;
using UnityEngine.Rendering;

public class ShaderManager : MonoBehaviour
{
    ShaderKeyword useVertexSnapping;
    ShaderKeyword useWorldCurve;
    ShaderKeyword useFog;
    ShaderKeyword useCoulds;
    public static ShaderManager instance;

    private void Awake()
    {
        //singelton instance
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    [Header("Shader Parameters")]
    [SerializeField] public ShaderParameters shaderParameters;
    [SerializeField] ShaderParameters defaultShaderParameters;
    public float lerpSpeed = 0.1f;
    
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

    [Button("Set Shader Parameters")]
    public void SetShaderParameters()
    {
        if (shaderParameters == null)
        {
            Debug.LogError("Shader Parameters not assigned!");
            return;
        }
        Shader.SetGlobalFloat("_FOG_DENSITY", shaderParameters.fogDensity);
        Shader.SetGlobalFloat("_FOG_HUE", shaderParameters.fogHue);
        Shader.SetGlobalFloat("_WAVE_AMPLITUDE", shaderParameters.waveAmplitude);
        Shader.SetGlobalFloat("_WATER_HUE", shaderParameters.waterHue);
        Shader.SetGlobalFloat("_CLOUD_DENSITY", shaderParameters.cloudDensity);
        Shader.SetGlobalFloat("_CLOUD_HUE", shaderParameters.cloudHue);
    }
    public void SetShaderParameters(ShaderParameters shaderParameters)
    {
        if(shaderParameters == null)
        {
            Debug.LogError("Shader Parameters not assigned!");
            return;
        }
        Shader.SetGlobalFloat("_FOG_DENSITY",  Mathf.Lerp(Shader.GetGlobalFloat("_FOG_DENSITY"), shaderParameters.fogDensity, Time.deltaTime * lerpSpeed));
        Shader.SetGlobalFloat("_FOG_HUE", Mathf.Lerp(Shader.GetGlobalFloat("_FOG_HUE"), shaderParameters.fogHue, Time.deltaTime * lerpSpeed));
        Shader.SetGlobalFloat("_WAVE_AMPLITUDE", Mathf.Lerp(Shader.GetGlobalFloat("_WAVE_AMPLITUDE"), shaderParameters.waveAmplitude, Time.deltaTime * lerpSpeed));
        Shader.SetGlobalFloat("_WATER_HUE", Mathf.Lerp(Shader.GetGlobalFloat("_WATER_HUE"), shaderParameters.waterHue, Time.deltaTime * lerpSpeed));
        Shader.SetGlobalFloat("_CLOUD_DENSITY", Mathf.Lerp(Shader.GetGlobalFloat("_CLOUD_DENSITY"), shaderParameters.cloudDensity, Time.deltaTime * lerpSpeed));
        Shader.SetGlobalFloat("_CLOUD_HUE", Mathf.Lerp(Shader.GetGlobalFloat("_CLOUD_HUE"), shaderParameters.cloudHue, Time.deltaTime * lerpSpeed)); 
    }

    //Update that lerps the shader parameters over time
    private void Update()
    {
        SetShaderParameters(shaderParameters);
    }

    public void SetShaderParametersAsset(ShaderParameters shaderParameters)
    {
        this.shaderParameters = shaderParameters;
    }
    public void SetShaderParametersAssetDefault()
    {
        shaderParameters = defaultShaderParameters;
    }
    //set back to defealt when exiting playmode
    private void OnDisable()
    {
        SetShaderParametersAssetDefault();
        SetShaderParameters();
    }

}


