using System;
using Unity.VisualScripting;
using UnityEngine;

public class TwirlJump : Ability
{
	public TwirlJump(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleTwirlJump();
	}
	void HandleTwirlJump()
	{
		if ((mov.AV.tj.twirlJumping && (mov.GCV.grounded && !mov.JC.jumping || mov.DV.dashing || mov.SUV.surfing)) || mov.AV.tj.turnOffTwirlJump)
		{
			mov.AV.tj.twirlJumping = false;
			mov.PlayerAnimator.SetBool("Twirling", false);
			PlayerVFX.instance.twirl.gameObject.SetActive(false);
			mov.AV.tj.turnOffTwirlJump = false;
		}
	}

	public override void ResetValues()
	{
		mov.AV.tj.twirlJumping = false;
		mov.AV.tj.turnOffTwirlJump = true;
		mov.PlayerAnimator.SetBool("Twirling", false);
	}

	public override void UpdateAnimator()
	{
		if (mov.AV.tj.twirlJumpAnimation)
		{
			mov.PlayerAnimator.SetBool("Twirling", true);
			mov.PlayerAnimator.SetTrigger("Twirl");
			mov.AV.tj.twirlJumpAnimation = false;
		}

	}
}
