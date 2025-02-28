using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class TalkingState : PlayerState
{
	Rigidbody rb;
	public InteractionVariables iv;
	private void Awake()
	{
		if (rb == null)
			rb = GetComponent<Rigidbody>();
	}

	private void Update()
	{
		CheckForInteractibles();
	}

	public override void EnterState()
	{
		base.EnterState();
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Interactions.Talk, Interact);
	}

	public override void ExitState()
	{
		base.ExitState();
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Interactions.Talk, Interact);

	}

	void CheckForInteractibles()
	{
		//do a physics sphere check around the player, and check if anything is interactible within that
		if (iv.interacting)
		{
			if (iv.currentInteractible != null)
				iv.currentInteractible.RemoveHighlight();
			return;
		}
		Collider[] collidersClose = Physics.OverlapSphere(rb.position, iv.measuringDistance);

		Interactible previousInteractable = iv.currentInteractible;
		Interactible closestInteractible = null;



		foreach (Collider collider in collidersClose)
		{
			if (collider.GetComponent<Interactible>() == null)
				continue;

			if (closestInteractible == null || Vector3.Distance(collider.transform.position, rb.transform.position) < Vector3.Distance(closestInteractible.transform.position, rb.transform.position))
			{
				closestInteractible = collider.GetComponent<Interactible>();
			}
		}

		iv.currentInteractible = closestInteractible;

		if (previousInteractable != null && previousInteractable != iv.currentInteractible)
			previousInteractable.RemoveHighlight();

		if (iv.currentInteractible != null)
			iv.currentInteractible.Highlight();
	}

	void Interact(InputAction.CallbackContext context)
	{
		iv.interacting = iv.currentInteractible.InteractWith();
	}
}
