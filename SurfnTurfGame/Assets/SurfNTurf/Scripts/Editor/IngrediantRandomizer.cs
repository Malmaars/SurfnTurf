using UnityEditor;
using UnityEngine;

public class IngrediantRandomizer : EditorWindow
{
    public int minID = 1;
    public int maxID = 16;
    [MenuItem("Tools/Randomize ingredients")]
    public static void ShowWindow()
    {
        GetWindow<IngrediantRandomizer>("Randomize ingredients");
    }
    private void OnGUI()
    {
        GUILayout.Label("Randomize ingredients", EditorStyles.boldLabel);

        minID = EditorGUILayout.IntField("min", minID);
        maxID = EditorGUILayout.IntField("max", maxID);

        if (GUILayout.Button("Randomize ingredients"))
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
                var pickUp = obj.GetComponent<PickUpIngredient>();
                if (pickUp != null)
                {
                    pickUp.LeftOptionData.ingredientID = Random.Range(minID, maxID);
                    pickUp.RightOptionData.ingredientID = Random.Range(minID, maxID);
                    EditorUtility.SetDirty(pickUp); // Mark as dirty so changes are saved
                }
            }
        }
    }

}
