using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomPropertyDrawer(typeof(Objective), true)]
public class ObjectiveDrawer : PropertyDrawer
{
	private static Type[] objectiveTypes;
	private static string[] objectiveTypeNames;

	static ObjectiveDrawer()
	{
		var baseType = typeof(Objective);
		var types = new List<Type>();
		foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
		{
			try
			{
				foreach (var type in assembly.GetTypes())
				{
					if (type.IsSubclassOf(baseType) && !type.IsAbstract)
					{
						types.Add(type);
					}
				}
			}
			catch { }
		}

		objectiveTypes = types.ToArray();
		objectiveTypeNames = Array.ConvertAll(objectiveTypes, t => t.Name);
	}

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float baseHeight = EditorGUI.GetPropertyHeight(property, true);
		return baseHeight + EditorGUIUtility.singleLineHeight + 4;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		Rect dropdownRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

		// Draw type selector
		int currentTypeIndex = -1;
		if (property.managedReferenceValue != null)
		{
			Type currentType = property.managedReferenceValue.GetType();
			currentTypeIndex = Array.FindIndex(objectiveTypes, t => t == currentType);
		}

		int selectedIndex = EditorGUI.Popup(dropdownRect, "Objective Type", currentTypeIndex, objectiveTypeNames);

		if (selectedIndex != currentTypeIndex)
		{
			Type newType = objectiveTypes[selectedIndex];
			property.managedReferenceValue = Activator.CreateInstance(newType);
			property.serializedObject.ApplyModifiedProperties();
			return;
		}

		if (property.managedReferenceValue != null)
		{
			Rect fieldRect = new Rect(position.x, position.y + EditorGUIUtility.singleLineHeight + 4, position.width, position.height);
			EditorGUI.indentLevel++;
			EditorGUI.PropertyField(fieldRect, property, true);
			EditorGUI.indentLevel--;
		}
	}
}
