using Steamworks;
using SurfnTurf;
using UnityEngine;

public class Tricks : WaterAbility
{
	public Tricks(WaterMovementController _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleTricks();	
	}

	public void HandleTricks()
	{
		if (mov.wtv.activeKickFlipTokens < mov.wtv.kickFlipTokensFromGround + mov.wtv.extraKickFlipTokens
			&& mov.wv.onWater)
			mov.wtv.activeKickFlipTokens = mov.wtv.kickFlipTokensFromGround + mov.wtv.extraKickFlipTokens;
		
		if (mov.wtv.activeShoveItTokens< mov.wtv.shoveItTokensFromGround+ mov.wtv.extraShoveItTokens
			&& mov.wv.onWater)
			mov.wtv.activeShoveItTokens = mov.wtv.shoveItTokensFromGround + mov.wtv.extraShoveItTokens;
	}

	public override void UpdateTimers()
	{
		mov.wtv.kickflipCooldownTimer = mov.wtv.kickflipCooldownTimer.TimerCountdown();
		mov.wtv.shoveItCooldownTimer = mov.wtv.shoveItCooldownTimer.TimerCountdown();
	}

	public override void UpdateAnimator()
	{
		if (mov.wtv.kickFlipAnimation)
		{
			mov.wtv.kickFlipAnimation = false;
			mov.animator.SetTrigger("SurfJump");
		}

		if (mov.wtv.shoveItAnimation)
		{
			mov.wtv.shoveItAnimation = false;
			mov.animator.SetTrigger("SurfSwipe");
		}
	}
}
