using System;
using FMODUnity;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PickUpUI : MonoBehaviour
{
    public Canvas canvas;
    public UIOption LeftOption;
    public UIOption RightOption;
    public bool uIActive = false;
    public EventReference feulSound;
    public EventReference ingrediantSound;
    public EventReference toolSound;

    private float shapePreviewSize = 100f;
    private float pieceOffset = 0.1f;
    private void Start()
    {
        canvas.enabled = false;
        LeftOption.fuelIcon.SetActive(false);
        LeftOption.toolIcon.SetActive(false);
        RightOption.fuelIcon.SetActive(false);
        RightOption.toolIcon.SetActive(false);
    }
    public void ShowUI(PickUpOptionData leftOptionData, PickUpOptionData rightOptionData)
    {
        uIActive = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UIManager.instance.SetVisibleUI(false);
        
        SetupUIOption(LeftOption, leftOptionData);
        SetupUIOption(RightOption, rightOptionData);

        EventSystem.current.SetSelectedGameObject(LeftOption.button.gameObject);

        canvas.enabled = true;
    }
    private void SetupUIOption(UIOption option, PickUpOptionData optionData)
    {
        option.title.text = optionData.title;
        option.description.text = optionData.description;

        switch (optionData.pickUpType)
        {
            case PickUpType.Ingredient:
                option.fuelIcon.SetActive(false);
                option.toolIcon.SetActive(false);
                option.previewContainer.SetActive(true);
                GeneratePreview(option.previewContainer, optionData.ingredientData);
                break;
            case PickUpType.Fuel:
                option.fuelIcon.SetActive(true);
                option.toolIcon.SetActive(false);
                option.previewContainer.SetActive(false);
                break;
            case PickUpType.Tool:
                option.fuelIcon.SetActive(false);
                option.toolIcon.SetActive(true);
                option.previewContainer.SetActive(false);
                break;
        }

        option.button.onClick.AddListener(() => OnOptionSelected(optionData));
        option.button.interactable = true;
    }

    private void OnOptionSelected(PickUpOptionData selectedOption)
    {
        switch (selectedOption.pickUpType)
        {
            case PickUpType.Ingredient:
                if (!BlackBoard.cookingDatabase.TryAddIngredient(selectedOption.ingredientData))
                {
                    Debug.LogError("Failed to add ingredient: " + selectedOption.ingredientData.ingredientName);
                    Debug.Break();
                }
                RuntimeManager.PlayOneShot(ingrediantSound);
                break;
            case PickUpType.Fuel:
                BlackBoard.challengeManager.AddTime(selectedOption.fuelCount);
                RuntimeManager.PlayOneShot(feulSound);
                break;
            case PickUpType.Tool:
                BlackBoard.cookingManager.AddSpoon(selectedOption.toolId);
                RuntimeManager.PlayOneShot(toolSound);
                break;
        }
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        UIManager.instance.SetVisibleUI(true);
        BlackBoard.playerVFX.pickUp.SendEvent("OnPlay");
        canvas.enabled = false;
        uIActive = false;

        LeftOption.button.onClick.RemoveAllListeners();
        RightOption.button.onClick.RemoveAllListeners();
        LeftOption.button.interactable = false;
        RightOption.button.interactable = false;
        LeftOption.fuelIcon.SetActive(false);
        LeftOption.toolIcon.SetActive(false);
        RightOption.fuelIcon.SetActive(false);
        RightOption.toolIcon.SetActive(false);
    }
    void GeneratePreview(GameObject ShapePreviewCanvas, IngredientData thisIngredientData)
    {
        if (ShapePreviewCanvas.transform.childCount > 0)
        {
            foreach (Transform child in ShapePreviewCanvas.transform)
            {
                Destroy(child.gameObject);
            }
        }
        //show a visual for the ingredient that correspond with it's shape in the grid
        int[,] ingredientShapeMap = CookingHelperFunctions.GetMapFrom1DArray(thisIngredientData);

        int ingredientWidth = thisIngredientData.rows;
        int ingredientHeight = thisIngredientData.columns;

        for (int x = 0; x < ingredientShapeMap.GetLength(0); x++)
        {
            for (int y = 0; y < ingredientShapeMap.GetLength(1); y++)
            {
                if (ingredientShapeMap[x, y] == 0)
                {
                    //zero (0) represents nothing, emptiness, the void.
                    continue;
                }
                CellData myData = BlackBoard.cookingDatabase.GetCellData(ingredientShapeMap[x, y]);

                //Generate Raw images according to the shape of the ingredient
                GameObject rawImageObject = new GameObject("PreviewCell");

                // Set its parent to the canvas
                rawImageObject.transform.SetParent(ShapePreviewCanvas.transform, false);

                // Add a RawImage component
                rawImageObject.AddComponent<CanvasRenderer>();
                RawImage rawImage = rawImageObject.AddComponent<RawImage>();


                //rawImage.texture = myData.cellTexture;
                rawImage.color = myData.color;

                // Adjust size and position (optional)
                RectTransform rectTransform = rawImageObject.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(shapePreviewSize, shapePreviewSize);

                //center the whole ingredient horizontally, and build it from bottom to top

                // Center horizontally by shifting by (width - 1) / 2
                float centeredX = (x - (ingredientShapeMap.GetLength(0) - 1) / 2f) * (shapePreviewSize + pieceOffset);

                // Invert Y positioning so it builds from bottom up
                float adjustedY = y * (shapePreviewSize + pieceOffset);

                rectTransform.anchoredPosition = new Vector2(centeredX, adjustedY);
            }
        }
    }
}

[System.Serializable]
public class UIOption
{
    public Button button;
    public TMP_Text title;
    public TMP_Text description;
    public GameObject previewContainer;
    public GameObject fuelIcon;
    public GameObject toolIcon;
}
