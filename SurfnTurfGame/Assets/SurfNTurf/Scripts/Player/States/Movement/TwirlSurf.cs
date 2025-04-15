using UnityEngine;

public class TwirlSurf : Ability
{
	public TwirlSurf(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleTwirlSurf();
	}

	void HandleTwirlSurf()
	{
		if(mov.AV.tsv.twirlSurfing)
		{
			//keep spinning I guess?

			//constantly destroy shit around you

			Collider[] collidersClose = Physics.OverlapSphere(mov.RB.position, mov.SWV.swipeRange);

			foreach (Collider collider in collidersClose)
			{
				if (collider.GetComponent<Destructible>() == null)
					continue;
				else
				{
					collider.GetComponent<Destructible>().Destruct(mov.RB.transform);
				}
			}

			if (mov.Velocity.magnitude < mov.AV.tsv.minimumVelocityMagnitude || !mov.SUV.surfing)
				mov.AV.tsv.twirlSurfing = false;
		}
	}

	public override void ResetValues()
	{
		mov.AV.tsv.twirlSurfing = false;
	}

	public override void UpdateAnimator()
	{
	}
}
