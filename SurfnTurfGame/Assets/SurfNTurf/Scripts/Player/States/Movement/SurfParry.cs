using UnityEngine;

public class SurfParry : Ability
{
	public SurfParry(MovementController _mov) : base(_mov) { }

	public override void UpdateAnimator()
	{
		SetAnimatorTriggers();
	}

	public override void SetAnimatorTriggers()
	{
		if (mov.lgv.ledgeGrabbing)
			return;

		if (mov.av.sp.parryAnimation)
		{
			mov.animator.SetTrigger("Parry");
			mov.av.sp.parryAnimation = false;
		}
	}
}
