using System.Collections;
using System.Timers;
using FMODUnity;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.VFX;

public class PickUpIngredient : Interactible
{
	public PickUpOptionData LeftOptionData;
	public PickUpOptionData RightOptionData;
	private float rotationSpeed = 250f;
	public bool hasInteracted;
	public EventReference pickUpItem;

	private void Start()
	{
		isActive = false;
	}
	public override void Update()
	{
		base.Update();
		GetComponentInChildren<VisualEffect>().enabled = isActive;
		transform.rotation = (Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0, rotationSpeed * Time.deltaTime, 0)));
	}
	public override void Highlight()
	{
		base.Highlight();
	}


	public override bool InteractWith()
	{
		if (hasInteracted)
			return false;
		base.InteractWith();
		UIManager.instance.pickUpUI.ShowUI(LeftOptionData, RightOptionData);
		RuntimeManager.PlayOneShot(pickUpItem);
		hasInteracted = true;
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
