using UnityEngine;

public class PickUpIngredient : Interactible
{
	[SerializeField]
	Canvas inputPromptCanvas;
	[SerializeField]
	Canvas ShapePreviewCanvas;

	[PickupIngredientDropdownAttribute]
	public int ingredientID;

	private void Awake()
	{
		GeneratePreview();
	}

	public override void Highlight()
	{
		base.Highlight();
		//show a button prompt and maybe an outline

		base.Highlight();
		//show a little button above the character that indicates interaction
		if (!inputPromptCanvas.gameObject.activeSelf)
			inputPromptCanvas.gameObject.SetActive(true);
		if (!ShapePreviewCanvas.gameObject.activeSelf)
			ShapePreviewCanvas.gameObject.SetActive(true);

		inputPromptCanvas.transform.position = transform.position + Vector3.up;

		inputPromptCanvas.transform.forward = (inputPromptCanvas.transform.position - Camera.main.transform.position).normalized;

		ShapePreviewCanvas.transform.position = transform.position + Vector3.up * 2;

		ShapePreviewCanvas.transform.forward = (ShapePreviewCanvas.transform.position - Camera.main.transform.position).normalized;

		//show a visual for the ingredient that correspond with it's shape in the grid
		BlackBoard.cookingDatabase.GetIngredientData(ingredientID);

	}

	void GeneratePreview()
	{

	}

	public override bool InteractWith()
	{
		base.InteractWith();
		//put this ingredient in the inventory, then remove it with an animation
		//Inventory.AddItem(this);
		return BlackBoard.cookingDatabase.TryAddIngredient(ingredientID);
	}

	public override void RemoveHighlight()
	{
		base.RemoveHighlight();
		inputPromptCanvas.gameObject.SetActive(false);
		ShapePreviewCanvas.gameObject.SetActive(false);
	}
}
