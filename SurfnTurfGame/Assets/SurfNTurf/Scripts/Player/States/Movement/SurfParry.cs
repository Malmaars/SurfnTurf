using UnityEngine;

public class SurfParry : Ability
{
	public SurfParry(MovementController _mov) : base(_mov) { }

	public override void UpdateAnimator()
	{
		if(mov.AV.sp.parryAnimation)
		{
			mov.PlayerAnimator.SetTrigger("Parry");
			mov.AV.sp.parryAnimation = false;
		}
	}
}
