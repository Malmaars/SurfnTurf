using System.Collections;
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
    public Transform playerTransform;
    private bool isRipplePlaying = false;
    [SerializeField] private AnimationCurve rippleCurve;
    [SerializeField] private Vector3 SunDirection = new Vector3(0.5f, -1f, 0.5f);
    [SerializeField] private ShaderKeyword SunDirectionKeyword;
    public bool renderingVFX = true;

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
        DefaultSettings();
    }
    private void DefaultSettings()
    {
        useVertexSnapping = new ShaderKeyword("_USE_VERTEXSNAPPING");
        useWorldCurve = new ShaderKeyword("_USE_WORLDCURVE");
        useFog = new ShaderKeyword("_USE_FOG");
        useCoulds = new ShaderKeyword("_USE_CLOUDS");
        SunDirectionKeyword = new ShaderKeyword("_SUN_DIRECTION");
        Shader.EnableKeyword(useWorldCurve.name);
        Shader.EnableKeyword(useFog.name);
        Shader.EnableKeyword(useCoulds.name);
        Shader.DisableKeyword(useVertexSnapping.name);
    }
    [Header("Shader Parameters")]
    [SerializeField] public ShaderParameters shaderParameters;
    [SerializeField] ShaderParameters defaultShaderParameters;
    public float lerpSpeed = 0.1f;
    private bool isRippleIdlePlaying;

    [Button("Toggle Vertex Snapping")]
    public void ToggleVertexSnapping()
    {
        ToggleShaderEffect(useVertexSnapping);
    }
    [Button("Toggle WorldCurve")]
    public void ToggleWorldCurve()
    {
        ToggleShaderEffect(useWorldCurve);
    }
    [Button("Toggle Fog")]
    public void ToggleFog()
    {
        ToggleShaderEffect(useFog);
    }
    [Button("Toggle Clouds")]
    public void ToggleClouds()
    {
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
        if (shaderParameters == null)
        {
            Debug.LogError("Shader Parameters not assigned!");
            return;
        }
        Shader.SetGlobalFloat("_FOG_DENSITY", Mathf.Lerp(Shader.GetGlobalFloat("_FOG_DENSITY"), shaderParameters.fogDensity, Time.deltaTime * lerpSpeed));
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
        Shader.SetGlobalVector(SunDirectionKeyword.name, SunDirection);
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
    public IEnumerator PlayRipple()
    {
        if (!renderingVFX) yield break; // Prevent multiple coroutines from running at the same time
        if (isRipplePlaying) yield break; // Prevent multiple coroutines from running at the same time
        isRipplePlaying = true;
        Shader.SetGlobalVector("_PLAYER_POSITION", playerTransform.position);
        Shader.SetGlobalFloat("_RIPPLE_TIME", 0.05f);
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime * 2f;
            Shader.SetGlobalFloat("_RIPPLE_TIME", Mathf.Lerp(0.05f, 0.7f, rippleCurve.Evaluate(progress)));
            yield return null;
        }
        Shader.SetGlobalFloat("_RIPPLE_TIME", 0.7f);
        isRipplePlaying = false;
    }

    public IEnumerator PlayRippleIdle()
    {
        if (!renderingVFX) yield break;
        if (isRipplePlaying) yield break; // Prevent multiple coroutines from running at the same time
        if (isRippleIdlePlaying) yield break; // Prevent multiple coroutines from running at the same time
        isRippleIdlePlaying = true;
        Shader.SetGlobalVector("_PLAYER_POSITION", playerTransform.position);
        Shader.SetGlobalFloat("_RIPPLE_TIME_IDLE", 0.05f);
        float progress = 0;
        while (progress < 1)
        {
            progress += Time.deltaTime * 0.3f;
            Shader.SetGlobalFloat("_RIPPLE_TIME_IDLE", Mathf.Lerp(0.05f, 0.7f, rippleCurve.Evaluate(progress)));
            yield return null;
        }
        Shader.SetGlobalFloat("_RIPPLE_TIME_IDLE", 0.7f);
        isRippleIdlePlaying = false;
    }
    [Button("Toggle VFX", EButtonEnableMode.Playmode)]
    public void renderVFX()
    {
        Camera cam = Camera.main;
        if (cam != null)
        {
            //toggle the vfx layer in the culling mask
            if (cam.cullingMask == (cam.cullingMask | (1 << LayerMask.NameToLayer("VFX"))))
            {
                cam.cullingMask &= ~(1 << LayerMask.NameToLayer("VFX"));
                renderingVFX = false;
            }
            else
            {
                cam.cullingMask |= (1 << LayerMask.NameToLayer("VFX"));
                renderingVFX = true;
            }
        }


    }

    private void Start()
    {
#if UNITY_EDITOR
#else
            renderVFX();
            StartCoroutine(StartVFXTimer(60*15f));
#endif
    }

    private IEnumerator StartVFXTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        renderVFX();
    }

}


