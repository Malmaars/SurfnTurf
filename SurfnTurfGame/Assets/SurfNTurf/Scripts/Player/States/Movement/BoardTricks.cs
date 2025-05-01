using SurfnTurf;
using UnityEngine;

public class BoardTricks : Ability
{
	public BoardTricks(MovementController _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleTricks();
	}

	void HandleTricks()
	{
		if (mov.av.btv.activeKickFlipTokens < mov.av.btv.kickFlipTokensFromGround + mov.av.btv.extraKickFlipTokens
			&& mov.gcv.grounded)
			mov.av.btv.activeKickFlipTokens = mov.av.btv.kickFlipTokensFromGround + mov.av.btv.extraKickFlipTokens;

		if (mov.av.btv.activeShoveItTokens < mov.av.btv.shoveItTokensFromGround + mov.av.btv.extraShoveItTokens
			&& mov.gcv.grounded)
			mov.av.btv.activeShoveItTokens = mov.av.btv.shoveItTokensFromGround + mov.av.btv.extraShoveItTokens;
	}

	public override void UpdateTimers()
	{
		mov.av.btv.kickflipCooldownTimer = mov.av.btv.kickflipCooldownTimer.TimerCountdown(); ;
		mov.av.btv.shoveItCooldownTimer = mov.av.btv.shoveItCooldownTimer.TimerCountdown(); ;
	}

	public override void UpdateAnimator()
	{
		if(mov.av.btv.kickFlipAnimation)
		{
			mov.av.btv.kickFlipAnimation = false;
			mov.animator.SetTrigger("SurfJump");
		}

		if (mov.av.btv.shoveItAnimation)
		{
			mov.av.btv.shoveItAnimation = false;
			mov.animator.SetTrigger("SurfSwipe");
		}
	}
}
