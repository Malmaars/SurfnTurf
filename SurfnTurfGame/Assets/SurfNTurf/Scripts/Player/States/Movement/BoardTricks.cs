using FMODUnity;
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
			&& (mov.gcv.grounded || mov.av.gv.grinding))
			mov.av.btv.activeKickFlipTokens = mov.av.btv.kickFlipTokensFromGround + mov.av.btv.extraKickFlipTokens;

		if (mov.av.btv.activeShoveItTokens < mov.av.btv.shoveItTokensFromGround + mov.av.btv.extraShoveItTokens
			&& (mov.gcv.grounded || mov.av.gv.grinding))
			mov.av.btv.activeShoveItTokens = mov.av.btv.shoveItTokensFromGround + mov.av.btv.extraShoveItTokens;
	}

	public override void UpdateTimers()
	{
		mov.av.btv.kickflipCooldownTimer = mov.av.btv.kickflipCooldownTimer.TimerCountdown();
		mov.av.btv.shoveItCooldownTimer = mov.av.btv.shoveItCooldownTimer.TimerCountdown(); 
	}

	public override void UpdateAnimator()
	{
		SetAnimatorTriggers();
	}

	public override void SetAnimatorTriggers()
	{
		if (mov.lgv.ledgeGrabbing)
			return;

		if (mov.av.btv.kickFlipAnimation)
		{
			mov.av.btv.kickFlipAnimation = false;
			RuntimeManager.PlayOneShot(mov.av.btv.trickSound);
			mov.animator.SetTrigger("SurfJump");
		}

		if (mov.av.btv.shoveItAnimation)
		{
			mov.av.btv.shoveItAnimation = false;
			RuntimeManager.PlayOneShot(mov.av.btv.trickSound);
			mov.animator.SetTrigger("SurfSwipe");
		}
	}
}
