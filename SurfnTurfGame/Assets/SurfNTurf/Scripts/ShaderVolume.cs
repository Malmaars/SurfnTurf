using UnityEngine;

public class ShaderVolume : MonoBehaviour
{
    [SerializeField] private ShaderParameters shaderParameters;
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
