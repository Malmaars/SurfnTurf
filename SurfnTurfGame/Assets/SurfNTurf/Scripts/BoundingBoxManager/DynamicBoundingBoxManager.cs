using UnityEngine;
using UnityEditor;

public class DynamicBoundingBoxManager : MonoBehaviour {

    public LayerMask layerMask;
    private Renderer[] renderers;
    private Bounds[] originalBounds;

    void Start() {
        // Find all renderers in the scene
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.InstanceID);
        // Filter out renderers based on the layer mask
        renderers = System.Array.FindAll(allRenderers, renderer => (layerMask.value & (1 << renderer.gameObject.layer)) != 0);
        originalBounds = new Bounds[renderers.Length];
        // Store the original bounds and positions
        for (int i = 0; i < renderers.Length; i++) {
            originalBounds[i] = renderers[i].bounds;
            Bounds newBounds = originalBounds[i];
            newBounds.size = new Vector3(999999999999f, 999999999999f, 999999999999f);
            renderers[i].bounds = newBounds;
        }
    }

}