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
    }
    public void ShowUI(PickUpOptionData leftOptionData, PickUpOptionData rightOptionData)
    {
        uIActive = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        UIManager.instance.SetVisibleUI(false);

        LeftOption.title.text = leftOptionData.title;
        LeftOption.description.text = leftOptionData.description;
        LeftOption.icon.gameObject.SetActive(leftOptionData.showIcon);
        LeftOption.icon.sprite = leftOptionData.icon;
        LeftOption.previewContainer.SetActive(leftOptionData.showPreview);
        if (leftOptionData.showPreview)
        {
            GeneratePreview(LeftOption.previewContainer, leftOptionData.ingredientData);
        }

        RightOption.title.text = rightOptionData.title;
        RightOption.description.text = rightOptionData.description;
        RightOption.icon.gameObject.SetActive(rightOptionData.showIcon);
        RightOption.icon.sprite = rightOptionData.icon;
        RightOption.previewContainer.SetActive(rightOptionData.showPreview);
        if (rightOptionData.showPreview)
        {
            GeneratePreview(RightOption.previewContainer, rightOptionData.ingredientData);
        }

        EventSystem.current.SetSelectedGameObject(LeftOption.button.gameObject);

        LeftOption.button.onClick.AddListener(() => OnOptionSelected(leftOptionData));
        RightOption.button.onClick.AddListener(() => OnOptionSelected(rightOptionData));
        LeftOption.button.interactable = true;
        RightOption.button.interactable = true;

        canvas.enabled = true;
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
    public Image icon;
    public GameObject previewContainer;
}
