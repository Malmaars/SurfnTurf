using UnityEngine;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using System.Collections.Generic;
using UnityEngine.UIElements;

[Overlay(typeof(SceneView), "Angle Overlay")]
public class AngleOverlay : Overlay, ICreateToolbar
{
    private const string path = "Assets/SurfNTurf/Materials/M_Debugangles.mat";
    private Material targetMaterial;
    private float alphaValue;
    private float angleValue;

    public IEnumerable<string> toolbarElements => new[] { "AngleOverlay/ShowMenuButton" };

    public override VisualElement CreatePanelContent()
    {
        VisualElement container = new VisualElement();
        container.style.width = 300; // Set the width of the overlay

        if (targetMaterial == null)
        {
            targetMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);
        }

        if (targetMaterial != null)
        {
            alphaValue = targetMaterial.GetFloat("_Alpha");
            angleValue = targetMaterial.GetFloat("_Angle");

            Label label = new Label("Edit Sliders");
            container.Add(label);

            VisualElement alphaContainer = new VisualElement();
            alphaContainer.style.flexDirection = FlexDirection.Row;
            alphaContainer.style.justifyContent = Justify.SpaceBetween;

            Slider alphaSlider = new Slider("_Alpha", 0f, 1f);
            alphaSlider.style.flexGrow = 1;
            alphaSlider.value = alphaValue;
            FloatField alphaField = new FloatField();
            alphaField.value = alphaValue;
            alphaField.style.width = 50;

            alphaSlider.RegisterValueChangedCallback(evt =>
            {
                alphaValue = evt.newValue;
                alphaField.value = alphaValue;
                targetMaterial.SetFloat("_Alpha", alphaValue);
            });

            alphaField.RegisterValueChangedCallback(evt =>
            {
                alphaValue = evt.newValue;
                alphaSlider.value = alphaValue;
                targetMaterial.SetFloat("_Alpha", alphaValue);
            });

            alphaContainer.Add(alphaSlider);
            alphaContainer.Add(alphaField);
            container.Add(alphaContainer);

            VisualElement angleContainer = new VisualElement();
            angleContainer.style.flexDirection = FlexDirection.Row;
            angleContainer.style.justifyContent = Justify.SpaceBetween;

            Slider angleSlider = new Slider("_Angle", 0f, 90f);
            angleSlider.style.flexGrow = 1;
            angleSlider.value = angleValue;
            FloatField angleField = new FloatField();
            angleField.value = angleValue;
            angleField.style.width = 50;

            angleSlider.RegisterValueChangedCallback(evt =>
            {
                angleValue = evt.newValue;
                angleField.value = angleValue;
                targetMaterial.SetFloat("_Angle", angleValue);
            });

            angleField.RegisterValueChangedCallback(evt =>
            {
                angleValue = evt.newValue;
                angleSlider.value = angleValue;
                targetMaterial.SetFloat("_Angle", angleValue);
            });

            angleContainer.Add(angleSlider);
            angleContainer.Add(angleField);
            container.Add(angleContainer);
        }
        else
        {
            Label errorLabel = new Label($"Material not found at path: {path}");
            container.Add(errorLabel);
        }

        return container;
    }

    public void OnToolbarGUI()
    {
        // This method can be left empty or used for additional toolbar GUI elements if needed
    }

    [EditorToolbarElement("AngleOverlay/ShowMenuButton", typeof(SceneView))]
    public class ShowMenuButton : EditorToolbarButton
    {
        public ShowMenuButton()
        {
            text = "Angle Overlay";
            clicked += ToggleOverlay;
        }

        private void ToggleOverlay()
        {
            AngleOverlayWindow.ShowWindow();
        }
    }
}

public class AngleOverlayWindow : EditorWindow
{
    private const string path = "Assets/SurfNTurf/Materials/M_Debugangles.mat";
    private Material targetMaterial;
    private float alphaValue;
    private float angleValue;

    [MenuItem("Window/Angle Overlay")]
    public static void ShowWindow()
    {
        AngleOverlayWindow window = GetWindow<AngleOverlayWindow>("Angle Overlay");
        window.Show();
    }

    private void OnEnable()
    {
        targetMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (targetMaterial != null)
        {
            alphaValue = targetMaterial.GetFloat("_Alpha");
            angleValue = targetMaterial.GetFloat("_Angle");
        }
    }

    private void OnGUI()
    {
        if (targetMaterial != null)
        {
            EditorGUILayout.LabelField("Edit Sliders", EditorStyles.boldLabel);

            alphaValue = EditorGUILayout.Slider("_Alpha", alphaValue, 0f, 1f);
            angleValue = EditorGUILayout.Slider("_Angle", angleValue, 0f, 360f);

            targetMaterial.SetFloat("_Alpha", alphaValue);
            targetMaterial.SetFloat("_Angle", angleValue);
        }
        else
        {
            EditorGUILayout.LabelField($"Material not found at path: {path}");
        }
    }
}


