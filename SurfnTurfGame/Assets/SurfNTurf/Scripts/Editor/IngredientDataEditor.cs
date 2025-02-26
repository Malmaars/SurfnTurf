using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(IngredientData))]
public class IngredientDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        IngredientData ingredientData = (IngredientData)target;

        ingredientData.id = EditorGUILayout.IntField("ID", ingredientData.id);
        ingredientData.name = EditorGUILayout.TextField("Name", ingredientData.name);

        EditorGUILayout.Space();

        // Update rows and columns
        int newRows = EditorGUILayout.IntField("Rows", ingredientData.rows);
        int newColumns = EditorGUILayout.IntField("Columns", ingredientData.columns);

        // If the shape array size is incorrect, resize it
        if (ingredientData.shape == null || ingredientData.shape.Length != newRows * newColumns)
        {
            ingredientData.shape = new int[newRows * newColumns];
        }

        // Store the new rows and columns in the ingredient data
        ingredientData.rows = newRows;
        ingredientData.columns = newColumns;

        EditorGUILayout.Space();

        // Display the flattened 2D grid in the inspector
        for (int row = 0; row < ingredientData.rows; row++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int col = 0; col < ingredientData.columns; col++)
            {
                // Convert 2D coordinates (row, col) to the flattened array index
                int index = row * ingredientData.columns + col;
                ingredientData.shape[index] = EditorGUILayout.IntField(ingredientData.shape[index], GUILayout.Width(30));
            }
            EditorGUILayout.EndHorizontal();
        }

        // Mark the object as dirty if the GUI has changed
        if (GUI.changed)
        {
            EditorUtility.SetDirty(ingredientData);
        }
    }

}