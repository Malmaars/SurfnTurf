using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Drawing.Printing;

public class IngrediantRandomizer : EditorWindow
{
    public string ingrediantFolder = "Assets/SurfNTurf/ScriptableObjects/Cooking/Ingredients"; // Set your folder path here

    [MenuItem("Tools/Randomize ingredients")]
    public static void ShowWindow()
    {
        GetWindow<IngrediantRandomizer>("Randomize ingredients");
    }

    private void OnGUI()
    {
        GUILayout.Label("Randomize ingredients", EditorStyles.boldLabel);

        ingrediantFolder = EditorGUILayout.TextField("Ingredient Folder", ingrediantFolder);

        if (GUILayout.Button("Randomize ingredients"))
        {
            SnapSelectedObjects();
        }
    }

    private void SnapSelectedObjects()
    {
        // Find all IngrediantData assets in the folder
        string[] guids = AssetDatabase.FindAssets("t:IngredientData", new[] { ingrediantFolder });
        List<IngredientData> allData = new List<IngredientData>();
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            IngredientData data = AssetDatabase.LoadAssetAtPath<IngredientData>(path);
            if (data != null)
            {
                allData.Add(data);
            }
        }

        if (allData.Count == 0)
        {
            Debug.LogWarning("No IngredientData assets found in folder: " + ingrediantFolder);
            return;
        }

        foreach (GameObject obj in Selection.gameObjects)
        {
            if (obj != null)
            {
                var pickUp = obj.GetComponent<PickUpIngredient>();
                if (pickUp != null)
                {
                    pickUp.LeftOptionData.ingredientData = allData[Random.Range(0, allData.Count)];
                    pickUp.RightOptionData.ingredientData = allData[Random.Range(0, allData.Count)];
                    EditorUtility.SetDirty(pickUp);
                }
            }
        }
    }
}
