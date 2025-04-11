using UnityEngine;

public class SurfParry : Ability
{
	public SurfParry(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		
	}

	void HandleSurfParry()
	{

	}

	public override void UpdateAnimator()
	{
		if(mov.AV.sp.parryAnimation)
		{
			mov.PlayerAnimator.SetTrigger("Parry");
			mov.AV.sp.parryAnimation = false;
		}
	}
}
