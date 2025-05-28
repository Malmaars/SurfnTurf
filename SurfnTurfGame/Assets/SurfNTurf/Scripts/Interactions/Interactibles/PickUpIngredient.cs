using System.Collections;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PickUpIngredient : Interactible
{
	public PickUpOptionData LeftOptionData;
	public PickUpOptionData RightOptionData;
	private float rotationSpeed = 250f;
	private void Start()
	{
		isActive = false;
	}
	public override void Update()
	{
		base.Update();
		transform.rotation = (Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, rotationSpeed * Time.deltaTime, 0)));
	}
	public override void Highlight()
	{
		base.Highlight();
	}


	public override bool InteractWith()
	{
		base.InteractWith();
		UIManager.instance.pickUpUI.ShowUI(LeftOptionData, RightOptionData);
		isActive = false;
		gameObject.SetActive(false);
		return false;
	}

	public override void RemoveHighlight()
	{
		base.RemoveHighlight();
	}
}

public enum PickUpType
{
	Ingredient,
	Fuel,
	Tool
}
[System.Serializable]
public class PickUpOptionData
{
	public PickUpType pickUpType;
	public string title;
	public string description;
	public bool showPreview;
	[PickupIngredientDropdown] public int ingredientID;
	public bool showIcon;
	public Sprite icon;
	public int fuelCount;
	public int toolId;
}
