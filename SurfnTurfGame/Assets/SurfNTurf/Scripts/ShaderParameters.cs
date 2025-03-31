using UnityEngine;

[CreateAssetMenu(fileName = "ShaderParameters", menuName = "Scriptable Objects/ShaderParameters", order = 1)]
public class ShaderParameters : ScriptableObject
{
    [Range(0,1)]public float fogDensity = 0f;
    [Range(0,1)]public float fogHue = 0f;
    [Range(0,1)]public float waveAmplitude = 0f;
    [Range(0,1)]public float waterHue = 0f;
    [Range(0,1)]public float cloudDensity = 0f;
    [Range(0,1)]public float cloudHue = 0f;
}
