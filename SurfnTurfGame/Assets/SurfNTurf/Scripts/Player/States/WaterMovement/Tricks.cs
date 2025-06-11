using FMODUnity;
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
			&& (mov.wv.onWater || mov.gv.grinding))
			mov.wtv.activeKickFlipTokens = mov.wtv.kickFlipTokensFromGround + mov.wtv.extraKickFlipTokens;
		
		if (mov.wtv.activeShoveItTokens< mov.wtv.shoveItTokensFromGround+ mov.wtv.extraShoveItTokens
			&& (mov.wv.onWater || mov.gv.grinding))
			mov.wtv.activeShoveItTokens = mov.wtv.shoveItTokensFromGround + mov.wtv.extraShoveItTokens;

		if (mov.wtv.activeBarrelRollTokens < mov.wtv.barrelRollTokensFromGround + mov.wtv.extraBarrelRollTokens
			&& (mov.wv.onWater || mov.gv.grinding))
			mov.wtv.activeBarrelRollTokens = mov.wtv.barrelRollTokensFromGround + mov.wtv.extraBarrelRollTokens;
	}

	public override void UpdateTimers()
	{
		mov.wtv.kickflipCooldownTimer = mov.wtv.kickflipCooldownTimer.TimerCountdown();
		mov.wtv.shoveItCooldownTimer = mov.wtv.shoveItCooldownTimer.TimerCountdown();
		mov.wtv.barrelRollCooldownTimer = mov.wtv.barrelRollCooldownTimer.TimerCountdown();
	}

	public override void UpdateAnimator()
	{
		if (mov.wtv.kickFlipAnimation)
		{
			mov.wtv.kickFlipAnimation = false;
			RuntimeManager.PlayOneShot(mov.wtv.trickSound);
			BlackBoard.playerVFX.trick.SendEvent("OnKickFlip");
			mov.animator.SetTrigger("SurfJump");
		}

		if (mov.wtv.shoveItAnimation)
		{
			mov.wtv.shoveItAnimation = false;
			RuntimeManager.PlayOneShot(mov.wtv.trickSound);
			BlackBoard.playerVFX.trick.SendEvent("OnShoveIt");
			mov.animator.SetTrigger("SurfSwipe");
		}
		if (mov.wtv.barrelRollAnimation)
		{
			mov.wtv.barrelRollAnimation = false;
			RuntimeManager.PlayOneShot(mov.wtv.trickSound);
			BlackBoard.playerVFX.trick.SendEvent("OnBarrelRoll");
			mov.animator.SetTrigger("SurfDash");
		}
	}
}
