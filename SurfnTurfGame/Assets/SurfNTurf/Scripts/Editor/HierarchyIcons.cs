//Author: David Jak
using System;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public class HierarchyIcons : MonoBehaviour
{
    static bool _hierarchyHasFocus = false;
    static EditorWindow _hierarchyEditorWindow;

    static HierarchyIcons() //Starts the functions
    {
        EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        EditorApplication.update += OnEditorUpdate;
    }

    private static void OnEditorUpdate() //Used to check if Hierarchy is in focus
    {
        if (EditorApplication.isPlaying || !IsAnyWindowMaximized())
            return;

        if (_hierarchyEditorWindow == null)
        {
            _hierarchyEditorWindow = EditorWindow.GetWindow(System.Type.GetType("UnityEditor.SceneHierarchyWindow,UnityEditor"));
        }

        _hierarchyHasFocus = EditorWindow.focusedWindow != null && EditorWindow.focusedWindow == _hierarchyEditorWindow;
    }
    static bool IsAnyWindowMaximized()
    {
        // Get all open editor windows
        EditorWindow[] windows = Resources.FindObjectsOfTypeAll<EditorWindow>();

        foreach (EditorWindow window in windows)
        {
            // Use reflection to access internal `maximized` property
            var isMaximized = typeof(EditorWindow).GetProperty("maximized", 
                                System.Reflection.BindingFlags.NonPublic | 
                                System.Reflection.BindingFlags.Instance);

            if (isMaximized != null && (bool)isMaximized.GetValue(window))
            {
                return true;
            }
        }
        return false;
    }

    static void DrawActivateToggle(Rect selectionRect, GameObject gameObject)
    {
        Rect toggleRect = new Rect(selectionRect);
        toggleRect.x -= 27f;
        toggleRect.width = 13f;
        bool active = EditorGUI.Toggle(toggleRect, gameObject.activeSelf);
        if (active != gameObject.activeSelf)
        {
            Undo.RecordObject(gameObject, "changing active state of game object");
            gameObject.SetActive(active);
            if (!EditorApplication.isPlaying)
            {
                EditorSceneManager.MarkSceneDirty(gameObject.scene);
            }
        }
    }
    private static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect) //Draws the icons and backgrounds
    {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj == null)
        {
            return;
        }

        DrawActivateToggle(selectionRect, obj);

        if (PrefabUtility.GetCorrespondingObjectFromOriginalSource(obj) != null)
        {
            return;
        }



        Component[] components = obj.GetComponents<Component>();
        if (components == null || components.Length == 0)
        {
            return;
        }

        Component component = components.Length > 1 ? components[1] : components[0];

        if(component == null)
            return;
        Type type = component.GetType();

        GUIContent content = EditorGUIUtility.ObjectContent(component, type); //Draws icons
        content.text = null;
        content.tooltip = type.Name;

        if (content.image == null)
        {
            return;
        }

        bool isSelected = Selection.instanceIDs.Contains(instanceID);
        bool isHovering = selectionRect.Contains(Event.current.mousePosition);

        Color color = UnityEditorBackgroundColor.Get(isSelected, isHovering, _hierarchyHasFocus);
        Rect backgroundRect = selectionRect;
        backgroundRect.width = 18.5f;
        EditorGUI.DrawRect(backgroundRect, color); //Draws background

        EditorGUI.LabelField(selectionRect, content); //Draws tooltip
    }

    public static class UnityEditorBackgroundColor //Used for the editor colors
    {
        static readonly Color defealtColor = new Color(0.7843f, 0.7843f, 0.7843f);
        static readonly Color defealtProColor = new Color(0.2196f, 0.2196f, 0.2196f);
        static readonly Color selectedColor = new Color(0.22745f, 0.447f, 0.6902f);
        static readonly Color selectedProColor = new Color(0.1725f, 0.3647f, 0.5394f);
        static readonly Color selectedUnFocusedColor = new Color(0.68f, 0.68f, 0.68f);
        static readonly Color selectedUnFocusedProColor = new Color(0.3f, 0.3f, 0.3f);
        static readonly Color hoveredColor = new Color(0.698f, 0.698f, 0.698f);
        static readonly Color hoveredProColor = new Color(0.2706f, 0.2706f, 0.2706f);

        public static Color Get(bool isSelected, bool isHoverd, bool isWindowFocussed)
        {
            if (isSelected)
            {
                if (isWindowFocussed)
                {
                    return EditorGUIUtility.isProSkin ? selectedProColor : selectedColor;
                }
                else
                {
                    return EditorGUIUtility.isProSkin ? selectedUnFocusedProColor : selectedUnFocusedColor;
                }
            }
            else if (isHoverd)
            {
                return EditorGUIUtility.isProSkin ? hoveredProColor : hoveredColor;
            }
            else
            {
                return EditorGUIUtility.isProSkin ? defealtProColor : defealtColor;
            }
        }
    }
}
