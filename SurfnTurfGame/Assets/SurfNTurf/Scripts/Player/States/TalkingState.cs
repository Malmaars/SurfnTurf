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

    public override void InitStateTransitions()
    {
        base.InitStateTransitions();
        transitions.Add(new PlayerStateTransition(typeof(PauseState), () => nextState == typeof(PauseState)));
        transitions.Add(new PlayerStateTransition(typeof(MovementController), () => nextState == typeof(MovementController)));

    }

    public override void EnterState()
	{
        iv.interacting = false;
		base.EnterState();
		CheckForInteractibles();
		InteractInit();

		//move the camera to a relevant position
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Interactions.Talk, Interact);
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Interactions.Exit, ExitInteract);
        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);

    }

    public override void ExitState()
	{
		base.ExitState();
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Interactions.Talk, Interact);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Interactions.Exit, ExitInteract);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);

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

        if (closestInteractible != null) ;
        iv.currentInteractible = closestInteractible;

        if (previousInteractable != null && previousInteractable != iv.currentInteractible)
            previousInteractable.RemoveHighlight();

        if (iv.currentInteractible != null)
            iv.currentInteractible.Highlight();
    }
    void InteractInit()
    {
        //Debug.Log("Run interactInit");
        if (iv.currentInteractible == null)
        {
            nextState = typeof(MovementController);
            return;
        }

		iv.interacting = iv.currentInteractible.InteractWith();

        if (!iv.interacting)
            nextState = typeof(MovementController);
    }


	void Interact(InputAction.CallbackContext context)
    {
        if (iv.currentInteractible == null)
        {
            nextState = typeof(MovementController);
			return;
		}

		iv.interacting = iv.currentInteractible.InteractWith();

        if (!iv.interacting)
			nextState = typeof(MovementController);	

	}

    void ExitInteract(InputAction.CallbackContext context)
    { 
        if (iv.currentInteractible == null)
            return;

        iv.interacting = iv.currentInteractible.Exit();

        if (!iv.interacting)
            nextState = typeof(MovementController);
    }
}
