using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter)), ExecuteInEditMode]
public class DynamicBoundingBox : MonoBehaviour {
    private MeshFilter meshFilter;
    private Vector3 originalPosition;
    private Bounds originalBounds;

    [SerializeField] private float expansionMultiplier = 2f; // Adjust this value if needed

    void Start() {
        meshFilter = GetComponent<MeshFilter>();

        if (meshFilter == null || meshFilter.mesh == null) {
            Debug.LogError("MeshFilter or Mesh missing on " + gameObject.name);
            return;
        }

        // Store the original bounds and position
        originalBounds = meshFilter.mesh.bounds;
        originalPosition = transform.position;
    }

    void Update() {
        if (meshFilter == null || meshFilter.mesh == null) return;

        Camera sceneCamera = SceneView.lastActiveSceneView?.camera;
        if (sceneCamera == null) return;

        // Calculate movement distance from the scene camera to the object
        float distance = Vector3.Distance(sceneCamera.transform.position, transform.position);

        // Expand bounds based on movement distance
        Bounds newBounds = originalBounds;
        newBounds.Expand(distance * expansionMultiplier);

        // Apply the updated bounds
        meshFilter.mesh.bounds = newBounds;
    }

    void OnDrawGizmosSelected() {
        if (meshFilter == null || meshFilter.mesh == null) return;

        // Draw the original bounds in green
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.TransformPoint(originalBounds.center), transform.TransformVector(originalBounds.size));

        // Draw the new bounds in red
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.TransformPoint(meshFilter.mesh.bounds.center), transform.TransformVector(meshFilter.mesh.bounds.size));
    }
}
