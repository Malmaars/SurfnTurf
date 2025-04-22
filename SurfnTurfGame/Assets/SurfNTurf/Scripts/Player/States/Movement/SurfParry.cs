using UnityEngine;

public class SurfParry : Ability
{
	public SurfParry(MovementController _mov) : base(_mov) { }

	public override void UpdateAnimator()
	{
		if(mov.av.sp.parryAnimation)
		{
			mov.animator.SetTrigger("Parry");
			mov.av.sp.parryAnimation = false;
		}
	}
}
