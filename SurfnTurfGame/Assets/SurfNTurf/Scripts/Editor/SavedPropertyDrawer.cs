using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(SavedProperty<>))]
public class SavedPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Draw the label
        position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

        // Get the "currentValue" field
        SerializedProperty currentValueProperty = property.FindPropertyRelative("defaultValue");

        if (currentValueProperty != null)
        {
            // Draw the current value field
            EditorGUI.PropertyField(position, currentValueProperty, GUIContent.none);
        }
        else
        {
            EditorGUI.LabelField(position, "Unsupported type");
        }
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("defaultValue"));
    }
}