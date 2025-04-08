using UnityEngine;
using UnityEditor;

public class RandomRotationTool : EditorWindow
{
    private Vector3 minRotation = Vector3.zero;
    private Vector3 maxRotation = Vector3.zero;
    private Vector3 minScale = Vector3.one;
    private Vector3 maxScale = Vector3.one;
    private bool uniformScale = false; // Toggle for uniform scaling

    [MenuItem("Tools/Random Rotation and Scale Tool")]
    public static void ShowWindow()
    {
        GetWindow<RandomRotationTool>("Random Rotation & Scale");
    }

    private void OnGUI()
    {
        GUILayout.Label("Random Rotation Settings", EditorStyles.boldLabel);

        minRotation = EditorGUILayout.Vector3Field("Min Rotation", minRotation);
        maxRotation = EditorGUILayout.Vector3Field("Max Rotation", maxRotation);

        if (GUILayout.Button("Randomize Rotations"))
        {
            RandomizeSelectedObjectsRotation();
        }

        GUILayout.Space(10);
        GUILayout.Label("Random Scale Settings", EditorStyles.boldLabel);

        minScale = EditorGUILayout.Vector3Field("Min Scale", minScale);
        maxScale = EditorGUILayout.Vector3Field("Max Scale", maxScale);

        uniformScale = EditorGUILayout.Toggle("Uniform Scale", uniformScale); // Toggle for uniform scaling

        if (GUILayout.Button("Randomize Scales"))
        {
            RandomizeSelectedObjectsScale();
        }
    }

    private void RandomizeSelectedObjectsRotation()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj.transform, "Randomize Rotation");
            obj.transform.rotation = Quaternion.Euler(
                Random.Range(minRotation.x, maxRotation.x),
                Random.Range(minRotation.y, maxRotation.y),
                Random.Range(minRotation.z, maxRotation.z)
            );
        }
    }

    private void RandomizeSelectedObjectsScale()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            Undo.RecordObject(obj.transform, "Randomize Scale");

            if (uniformScale)
            {
                // Generate a single random value for uniform scaling
                float randomScale = Random.Range(minScale.x, maxScale.x);
                obj.transform.localScale = new Vector3(randomScale, randomScale, randomScale);
            }
            else
            {
                // Generate random values for each axis
                obj.transform.localScale = new Vector3(
                    Random.Range(minScale.x, maxScale.x),
                    Random.Range(minScale.y, maxScale.y),
                    Random.Range(minScale.z, maxScale.z)
                );
            }
        }
    }
}