using UnityEngine;

public class TwirlSurf : Ability
{
	public TwirlSurf(MovementController _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleTwirlSurf();
	}

	void HandleTwirlSurf()
	{
		if(mov.av.tsv.twirlSurfing)
		{
			//keep spinning I guess?

			//constantly destroy shit around you

			Collider[] collidersClose = Physics.OverlapSphere(mov.rb.position, mov.swv.swipeRange);

			foreach (Collider collider in collidersClose)
			{
				if (collider.GetComponent<Destructible>() == null)
					continue;
				else
				{
					collider.GetComponent<Destructible>().Destruct(mov.rb.transform);
				}
			}

			if (mov.velocity.magnitude < mov.av.tsv.minimumVelocityMagnitude || !mov.suv.surfing)
				mov.av.tsv.twirlSurfing = false;
		}
	}

	public override void ResetValues()
	{
		mov.av.tsv.twirlSurfing = false;
	}

	public override void UpdateAnimator()
	{
		mov.animator.SetBool("SwipeSurfing", mov.av.tsv.twirlSurfing);

		SetAnimatorTriggers();
	}

	public override void SetAnimatorTriggers()
	{
		if (mov.lgv.ledgeGrabbing)
			return;

		if (mov.av.tsv.twirlSurfAnimation)
		{
			mov.av.tsv.twirlSurfAnimation = false;
			mov.animator.SetTrigger("SwipeSurf");
		}
	}
}
