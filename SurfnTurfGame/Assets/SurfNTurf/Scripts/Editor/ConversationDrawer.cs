using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(Conversation))]
public class ConversationDrawer : PropertyDrawer
{
	private bool questOptionsExpanded = true; // remember Quest Options foldout state

	public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
	{
		float height = EditorGUIUtility.singleLineHeight; // foldout line

		if (property.isExpanded)
		{
			height += GetFieldHeight(property, "myName");
			height += GetFieldHeight(property, "sentences");
			height += GetFieldHeight(property, "automaticallyGoesToNextConvo");
			height += GetFieldHeight(property, "hasQuest");

			SerializedProperty hasQuest = property.FindPropertyRelative("hasQuest");
			if (hasQuest.boolValue)
			{
				height += EditorGUIUtility.singleLineHeight; // space for "Quest Options" foldout label

				if (questOptionsExpanded)
				{
					height += GetFieldHeight(property, "quest");
					height += GetFieldHeight(property, "menuOpenCamera");
					height += GetFieldHeight(property, "QuestCompletedCamera");
					height += GetFieldHeight(property, "completedQuestSentences");
					height += GetFieldHeight(property, "QuestFailedCamera");
					height += GetFieldHeight(property, "failedQuestSentences");
					height += GetFieldHeight(property, "IDontWantADishCamera");
					height += GetFieldHeight(property, "IDontWantADishSentences");
				}
			}
		}

		return height;
	}

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
	{
		EditorGUI.BeginProperty(position, label, property);

		Rect foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
		property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, property.displayName, true);

		if (property.isExpanded)
		{
			EditorGUI.indentLevel++;
			float y = foldoutRect.y + EditorGUIUtility.singleLineHeight;

			y = DrawField(property, "myName", position, y);
			y = DrawField(property, "sentences", position, y);
			y = DrawField(property, "automaticallyGoesToNextConvo", position, y);
			y = DrawField(property, "hasQuest", position, y);

			SerializedProperty hasQuest = property.FindPropertyRelative("hasQuest");
			if (hasQuest.boolValue)
			{
				// Quest Options foldout
				Rect questFoldoutRect = new Rect(position.x, y, position.width, EditorGUIUtility.singleLineHeight);
				questOptionsExpanded = EditorGUI.Foldout(questFoldoutRect, questOptionsExpanded, "Quest Options", true);
				y += EditorGUIUtility.singleLineHeight;

				if (questOptionsExpanded)
				{
					EditorGUI.indentLevel++;

					y = DrawField(property, "quest", position, y);
					y = DrawField(property, "menuOpenCamera", position, y);
					y = DrawField(property, "QuestCompletedCamera", position, y);
					y = DrawField(property, "completedQuestSentences", position, y);
					y = DrawField(property, "QuestFailedCamera", position, y);
					y = DrawField(property, "failedQuestSentences", position, y);
					y = DrawField(property, "IDontWantADishCamera", position, y);
					y = DrawField(property, "IDontWantADishSentences", position, y);

					EditorGUI.indentLevel--;
				}
			}

			EditorGUI.indentLevel--;
		}

		EditorGUI.EndProperty();
	}

	private float DrawField(SerializedProperty parent, string name, Rect position, float y)
	{
		SerializedProperty prop = parent.FindPropertyRelative(name);
		if (prop == null) return y;

		float height = EditorGUI.GetPropertyHeight(prop, true);

		EditorGUI.PropertyField(
			new Rect(position.x, y, position.width, height),
			prop,
			true
		);

		return y + height;
	}

	private float GetFieldHeight(SerializedProperty parent, string name)
	{
		SerializedProperty prop = parent.FindPropertyRelative(name);
		if (prop == null) return 0;

		return EditorGUI.GetPropertyHeight(prop, true);
	}
}
