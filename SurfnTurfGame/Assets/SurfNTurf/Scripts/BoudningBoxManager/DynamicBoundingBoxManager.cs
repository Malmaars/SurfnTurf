using UnityEngine;
using UnityEditor;

public class DynamicBoundingBoxManager : MonoBehaviour {
    [SerializeField] private float expansionMultiplier = 2f; // Adjust this value if needed

    private Renderer[] renderers;
    private Bounds[] originalBounds;
    private Vector3[] originalPositions;

    void Start() {
        // Find all renderers in the scene
        Renderer[] allRenderers = FindObjectsByType<Renderer>(FindObjectsSortMode.InstanceID);
        // Filter out renderers with the tag "Player"
        renderers = System.Array.FindAll(allRenderers, renderer => renderer.gameObject.tag != "Player");

        originalBounds = new Bounds[renderers.Length];
        originalPositions = new Vector3[renderers.Length];

        // Store the original bounds and positions
        for (int i = 0; i < renderers.Length; i++) {
            originalBounds[i] = renderers[i].bounds;
            originalPositions[i] = renderers[i].transform.position;
        }
    }

    void Update() {
#if UNITY_EDITOR
        Camera sceneCamera = SceneView.lastActiveSceneView?.camera;
#else
        Camera sceneCamera = Camera.main;
#endif
        if (sceneCamera == null) return;

        // Update bounds for all renderers
        for (int i = 0; i < renderers.Length; i++) {
            Renderer renderer = renderers[i];
            if (renderer == null) continue;

            // Calculate movement distance from the scene camera to the object
            float distance = Vector3.Distance(sceneCamera.transform.position, renderer.transform.position);

            // Expand bounds based on movement distance
            Bounds newBounds = originalBounds[i];
            newBounds.Expand(distance * expansionMultiplier);

            // Apply the updated bounds
            renderer.bounds = newBounds;
        }
    }

    void OnDrawGizmosSelected() {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++) {
            Renderer renderer = renderers[i];
            if (renderer == null) continue;

            // Draw the original bounds in green
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(originalBounds[i].center, originalBounds[i].size);

            // Draw the new bounds in red
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(renderer.bounds.center, renderer.bounds.size);
        }
    }
}