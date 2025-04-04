using UnityEngine;
using UnityEditor;
using System.Linq;

[CustomPropertyDrawer(typeof(PickupIngredientDropdownAttribute))]
public class PickupIngredientDropdownDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.PropertyField(position, property, label);
            return;
        }

        // Load the CookingDatabase
        CookingDatabase database = Resources.Load<CookingDatabase>("CookingDatabase");
        if (database == null)
        {
            EditorGUI.LabelField(position, label.text, "CookingDatabase not found!");
            return;
        }

        var ingredientDatas = database.ingredientDatas;
        if (ingredientDatas == null || ingredientDatas.Count == 0)
        {
            EditorGUI.LabelField(position, label.text, "No ingredient data found!");
            return;
        }

        string[] options = ingredientDatas.Select(data => $"{data.id}: {data.ingredientName}").ToArray();

        int currentId = property.intValue;
        int selectedIndex = ingredientDatas.FindIndex(data => data.id == currentId);

        if (selectedIndex == -1) selectedIndex = 0;

        // Handle multi-object editing mixed value state
        EditorGUI.BeginProperty(position, label, property);
        EditorGUI.showMixedValue = property.hasMultipleDifferentValues;

        EditorGUI.BeginChangeCheck();
        int newIndex = EditorGUI.Popup(position, label.text, selectedIndex, options);
        if (EditorGUI.EndChangeCheck())
        {
            // Only apply the new value if the user actually changed something
            property.intValue = ingredientDatas[newIndex].id;
        }

        EditorGUI.showMixedValue = false;
        EditorGUI.EndProperty();
    }
}
