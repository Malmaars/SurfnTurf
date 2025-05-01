using NaughtyAttributes;
using UnityEngine;

public class ShaderVolume : MonoBehaviour
{
    [SerializeField,Expandable] private ShaderParameters shaderParameters;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ShaderManager.instance.SetShaderParametersAsset(shaderParameters);
        }
    }
    private void OnTriggerExit(Collider other)
    {
         ShaderManager.instance.SetShaderParametersAssetDefault();
    }

}
