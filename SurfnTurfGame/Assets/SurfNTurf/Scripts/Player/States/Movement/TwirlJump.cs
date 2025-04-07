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
		if (mov.GCV.grounded && !mov.JC.jumping || mov.DV.dashing || mov.SUV.surfing)
		{
			mov.AV.tj.twirlJumping = false;
			mov.PlayerAnimator.SetBool("Twirling", false);
			PlayerVFX.instance.twirl.gameObject.SetActive(false);
		}
	}
}
