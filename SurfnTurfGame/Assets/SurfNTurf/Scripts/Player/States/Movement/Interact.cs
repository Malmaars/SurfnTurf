using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Interact : Ability
{
	public Interact(MovementController _mov) : base(_mov) { }

	public override void RunOnEnterState()
	{
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Interactions.Talk, DoInteract);
	}

	public override void RunOnExitState()
	{
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Interactions.Talk, DoInteract);
	}

	public override void RunOnUpdateAfterSetVelocity()
	{
		CheckForInteractibles();
	}

	public override void RunOnDrawGizmos()
	{

		if (mov.iv.gizmosOn)
		{
			Gizmos.color = Color.blue;

			Gizmos.DrawWireSphere(mov.rb.position, mov.iv.measuringDistance);
		}
	}
	void CheckForInteractibles()
	{
		//do a physics sphere check around the player, and check if anything is interactible within that
		if (mov.iv.interacting)
		{
			if (mov.iv.currentInteractible != null)
				mov.iv.currentInteractible.RemoveHighlight();
			return;
		}
		Collider[] collidersClose = Physics.OverlapSphere(mov.rb.position, mov.iv.measuringDistance);

		Interactible previousInteractable = mov.iv.currentInteractible;
		Interactible closestInteractible = null;

		foreach (Collider collider in collidersClose)
		{
			if (collider.GetComponent<Interactible>() == null)
				continue;

			if (closestInteractible == null || Vector3.Distance(collider.transform.position, mov.rb.transform.position) < Vector3.Distance(closestInteractible.transform.position, mov.rb.transform.position))
			{
				closestInteractible = collider.GetComponent<Interactible>();
			}
		}

		mov.iv.currentInteractible = closestInteractible;

		if (previousInteractable != null && previousInteractable != mov.iv.currentInteractible)
			previousInteractable.RemoveHighlight();

		if (mov.iv.currentInteractible != null)
			mov.iv.currentInteractible.Highlight();
	}

	void DoInteract(InputAction.CallbackContext context)
	{
		if (mov.iv.currentInteractible == null)
			return;

		else
		{
			mov.SetNextState(typeof(TalkingState));
		}
		
	}
}
