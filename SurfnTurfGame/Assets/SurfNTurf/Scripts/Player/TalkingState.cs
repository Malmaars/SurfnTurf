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

    public override void InitStateTransitions()
    {
        base.InitStateTransitions();
        transitions.Add(new PlayerStateTransition(typeof(MovementController), () => nextState == typeof(MovementController)));
    }

    public override void EnterState()
	{
		base.EnterState();
		CheckForInteractibles();

		InteractInit();

		//move the camera to a relevant position
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Interactions.Talk, Interact);
	}

	public override void ExitState()
	{
		base.ExitState();
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Interactions.Talk, Interact);
	}

	void CheckForInteractibles()
	{
		Collider[] collidersClose = Physics.OverlapSphere(rb.position, iv.measuringDistance);
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
		iv.currentInteractible.RemoveHighlight();
	}

    void InteractInit()
    {
        if (iv.currentInteractible == null)
            return;
        iv.interacting = iv.currentInteractible.InteractWith();

        if (!iv.interacting)
            nextState = typeof(MovementController);

    }


	void Interact(InputAction.CallbackContext context)
    {
        if (iv.currentInteractible == null)
            return;
        iv.interacting = iv.currentInteractible.InteractWith();

        if (!iv.interacting)
			nextState = typeof(MovementController);	

	}
}
