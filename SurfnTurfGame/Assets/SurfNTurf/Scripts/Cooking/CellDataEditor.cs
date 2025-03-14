#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CellData))]
public class CellDataEditor : Editor
{
    private SerializedProperty idProperty;
    private SerializedProperty nameProperty;
    private SerializedProperty colorProperty;
    private SerializedProperty textureProperty;

    private void OnEnable()
    {
        idProperty = serializedObject.FindProperty("id");
        nameProperty = serializedObject.FindProperty("cellName");
        colorProperty = serializedObject.FindProperty("color");
        textureProperty = serializedObject.FindProperty("cellTexture");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // Draw all properties
        EditorGUILayout.PropertyField(idProperty);
        EditorGUILayout.PropertyField(nameProperty);
        EditorGUILayout.PropertyField(colorProperty);
        EditorGUILayout.PropertyField(textureProperty);

        // Apply changes and rename asset if needed
        if (serializedObject.hasModifiedProperties)
        {
            serializedObject.ApplyModifiedProperties();
            RenameAsset((CellData)target);
        }
    }

    private void RenameAsset(CellData cellData)
    {
        string assetPath = AssetDatabase.GetAssetPath(cellData);
        if (!string.IsNullOrEmpty(assetPath))
        {
            string newName = $"SO_C{cellData.id}_{cellData.cellName}";
            string currentName = System.IO.Path.GetFileNameWithoutExtension(assetPath);

            if (currentName != newName)
            {
                AssetDatabase.RenameAsset(assetPath, newName);
                AssetDatabase.SaveAssets();
            }
        }
    }
}

#endif