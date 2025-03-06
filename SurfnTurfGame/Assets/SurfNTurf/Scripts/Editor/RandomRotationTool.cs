using UnityEngine;
using UnityEditor;

public class RandomRotationTool : EditorWindow
{
    private Vector3 minRotation = Vector3.zero;
    private Vector3 maxRotation = Vector3.zero;
    
    [MenuItem("Tools/Random Rotation Tool")]
    public static void ShowWindow()
    {
        GetWindow<RandomRotationTool>("Random Rotation");
    }

    private void OnGUI()
    {
        GUILayout.Label("Random Rotation Settings", EditorStyles.boldLabel);

        minRotation = EditorGUILayout.Vector3Field("Min Rotation", minRotation);
        maxRotation = EditorGUILayout.Vector3Field("Max Rotation", maxRotation);
        
        if (GUILayout.Button("Randomize Rotations"))
        {
            RandomizeSelectedObjects();
        }
    }

    private void RandomizeSelectedObjects()
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
}