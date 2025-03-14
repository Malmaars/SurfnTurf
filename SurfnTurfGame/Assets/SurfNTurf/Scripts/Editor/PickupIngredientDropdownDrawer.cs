using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;


[CustomPropertyDrawer(typeof(PickupIngredientDropdownAttribute))]
public class PickupIngredientDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Ensure this is an int field
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // Find the CookingDatabase prefab
        CookingDatabase database = Resources.Load<CookingDatabase>("CookingDatabase");
        if (database == null)
        {
            EditorGUI.LabelField(position, label.text, "CookingDatabase not found!");
            return;
        }

        // Get the list of ingredient data
        var ingredientDatas = database.ingredientDatas;
        if (ingredientDatas == null || ingredientDatas.Count == 0)
        {
            EditorGUI.LabelField(position, label.text, "No ingredient data found!");
            return;
        }

        string[] options = ingredientDatas.Select(data => $"{data.id}: {data.ingredientName}").ToArray();

        // Adjust index to match ingredient ID system (starting from 1)
        int currentId = property.intValue;
        int selectedIndex = ingredientDatas.FindIndex(data => data.id == currentId);

        // Ensure the index is valid
        if (selectedIndex == -1)
            selectedIndex = 0;

        // Dropdown selection
        selectedIndex = EditorGUI.Popup(position, label.text, selectedIndex, options);
        property.intValue = ingredientDatas[selectedIndex].id;
    }
}