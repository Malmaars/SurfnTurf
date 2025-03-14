using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PickUpIngredient : Interactible
{
	[SerializeField]
	Canvas inputPromptCanvas;
	[SerializeField]
	Canvas ShapePreviewCanvas;

	public bool inputPromptOn, previewOn;

	bool previewGenerated;

	public Vector3 basePromptOffset, shapePreviewOffset;

	public float shapePreviewSize;
	public float pieceOffset;

	[PickupIngredientDropdown]
	public int ingredientID;

	public override void Highlight()
	{
		base.Highlight();

		if (!previewGenerated)
		{
			GeneratePreview();
			previewGenerated = true;
		}
		//show a little button above the character that indicates interaction
		if (inputPromptOn)
		{
			if (!inputPromptCanvas.gameObject.activeSelf)
				inputPromptCanvas.gameObject.SetActive(true);

			inputPromptCanvas.transform.position = transform.position + basePromptOffset + Vector3.up;

			inputPromptCanvas.transform.forward = (inputPromptCanvas.transform.position - Camera.main.transform.position).normalized;

		}

		if (previewOn)
		{
			if (!ShapePreviewCanvas.gameObject.activeSelf)
				ShapePreviewCanvas.gameObject.SetActive(true);

			int extraOffset = inputPromptOn ? 0 : 1;

			ShapePreviewCanvas.transform.position = transform.position + shapePreviewOffset + basePromptOffset + Vector3.down * extraOffset;

			ShapePreviewCanvas.transform.forward = (ShapePreviewCanvas.transform.position - Camera.main.transform.position).normalized;
		}
	}

	void GeneratePreview()
	{
		//show a visual for the ingredient that correspond with it's shape in the grid
		IngredientData thisIngredientData = BlackBoard.cookingDatabase.GetIngredientData(ingredientID);
		int[,] ingredientShapeMap = CookingHelperFunctions.GetMapFrom1DArray(thisIngredientData);

		int ingredientWidth = thisIngredientData.rows;
		int ingredientHeight = thisIngredientData.columns;

		for (int x = 0; x < ingredientShapeMap.GetLength(0); x++)
		{
			for (int y = 0; y < ingredientShapeMap.GetLength(1); y++) {
				if (ingredientShapeMap[x,y] == 0)
				{
					//zero (0) represents nothing, emptiness, the void.
					continue;
				}
				CellData myData = BlackBoard.cookingDatabase.GetCellData(ingredientShapeMap[x,y]);

				//Generate Raw images according to the shape of the ingredient
				GameObject rawImageObject = new GameObject("PreviewCell");

				// Set its parent to the canvas
				rawImageObject.transform.SetParent(ShapePreviewCanvas.transform, false);

				// Add a RawImage component
				rawImageObject.AddComponent<CanvasRenderer>();
				RawImage rawImage = rawImageObject.AddComponent<RawImage>();


				rawImage.texture = myData.cellTexture;
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

	public override bool InteractWith()
	{
		base.InteractWith();
		//put this ingredient in the inventory, then remove it with an animation
		//Inventory.AddItem(this);
		if (BlackBoard.cookingDatabase.TryAddIngredient(ingredientID))
		{
			//TEMP
			Destroy(this.gameObject);
		}

		else
		{
			//throw an INVENTORY FULL prompt
		}

		return false;
	}

	public override void RemoveHighlight()
	{
		base.RemoveHighlight();
		inputPromptCanvas.gameObject.SetActive(false);
		ShapePreviewCanvas.gameObject.SetActive(false);
	}
}
