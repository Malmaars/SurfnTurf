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
        
        Camera sceneCamera = Camera.main;
        if (sceneCamera == null) return;

        // Update bounds for all renderers
        for (int i = 0; i < renderers.Length; i++) {
            Renderer renderer = renderers[i];
            if (renderer == null) continue;

            Vector2 worldPosition = new Vector2(renderer.transform.position.x, renderer.transform.position.z);
            Vector2 cameraPosition = new Vector2(sceneCamera.transform.position.x, sceneCamera.transform.position.z);
            // Calculate movement distance from the scene camera to the object
            float distance = Vector2.Distance(cameraPosition, worldPosition);
            distance = Mathf.Pow(distance, 2);
            float multiplier = -(10*1E-05f);
            distance = multiplier * distance;

            Bounds newBounds = originalBounds[i];
            newBounds.center = new Vector3(originalBounds[i].center.x, originalBounds[i].center.y + distance, originalBounds[i].center.z);

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