using UnityEngine;

public class PickUpIngredient : Interactible
{
	[SerializeField]
	Canvas inputPromptCanvas;
	public override void Highlight()
	{
		base.Highlight();
		//show a button prompt and maybe an outline

		base.Highlight();
		//show a little button above the character that indicates interaction
		if (!inputPromptCanvas.gameObject.activeSelf)
			inputPromptCanvas.gameObject.SetActive(true);

		inputPromptCanvas.transform.position = transform.position + Vector3.up;

		inputPromptCanvas.transform.forward = (inputPromptCanvas.transform.position - Camera.main.transform.position).normalized;

	}

	public override bool InteractWith()
	{
		return base.InteractWith();
		//put this ingredient in the inventory, then remove it with an animation
	}

	public override void RemoveHighlight()
	{
		base.RemoveHighlight();
		inputPromptCanvas.gameObject.SetActive(false);
	}
}
