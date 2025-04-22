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

		if (mov.IV.gizmosOn)
		{
			Gizmos.color = Color.blue;

			Gizmos.DrawWireSphere(mov.RB.position, mov.IV.measuringDistance);
		}
	}
	void CheckForInteractibles()
	{
		//do a physics sphere check around the player, and check if anything is interactible within that
		if (mov.IV.interacting)
		{
			if (mov.IV.currentInteractible != null)
				mov.IV.currentInteractible.RemoveHighlight();
			return;
		}
		Collider[] collidersClose = Physics.OverlapSphere(mov.RB.position, mov.IV.measuringDistance);

		Interactible previousInteractable = mov.IV.currentInteractible;
		Interactible closestInteractible = null;

		foreach (Collider collider in collidersClose)
		{
			if (collider.GetComponent<Interactible>() == null)
				continue;

			if (closestInteractible == null || Vector3.Distance(collider.transform.position, mov.RB.transform.position) < Vector3.Distance(closestInteractible.transform.position, mov.RB.transform.position))
			{
				closestInteractible = collider.GetComponent<Interactible>();
			}
		}

		mov.IV.currentInteractible = closestInteractible;

		if (previousInteractable != null && previousInteractable != mov.IV.currentInteractible)
			previousInteractable.RemoveHighlight();

		if (mov.IV.currentInteractible != null)
			mov.IV.currentInteractible.Highlight();
	}

	void DoInteract(InputAction.CallbackContext context)
	{
		if (mov.IV.currentInteractible == null)
			return;
		mov.SetNextState(typeof(TalkingState));
	}
}
