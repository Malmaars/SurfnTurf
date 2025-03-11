using UnityEngine;
using UnityEditor;

public class SnapToGroundEditor : EditorWindow
{
    private float raycastDistance = 100f;
    
    [MenuItem("Tools/Snap To Ground")]
    public static void ShowWindow()
    {
        GetWindow<SnapToGroundEditor>("Snap To Ground");
    }

    private void OnGUI()
    {
        GUILayout.Label("Snap Objects to Ground", EditorStyles.boldLabel);

        raycastDistance = EditorGUILayout.FloatField("Raycast Distance", raycastDistance);

        if (GUILayout.Button("Snap Selected Objects"))
        {
            SnapSelectedObjects();
        }
    }

    private void SnapSelectedObjects()
    {
        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj != null)
            {
                RaycastHit hit;
                if (Physics.Raycast(obj.transform.position, Vector3.down, out hit, raycastDistance))
                {
                    Undo.RecordObject(obj.transform, "Snap to Ground");
                    obj.transform.position = new Vector3(obj.transform.position.x, hit.point.y, obj.transform.position.z);
                }
                else
                {
                    Debug.LogWarning($"{obj.name} did not move: No ground detected within {raycastDistance} units.", obj);
                }
            }
        }
    }
}