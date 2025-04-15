using UnityEngine;

public class BoardTricks : Ability
{
	public BoardTricks(IMovement _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleTricks();
	}

	void HandleTricks()
	{
		if (mov.AV.btv.kickflipCooldownTimer > 0)
			mov.AV.btv.kickflipCooldownTimer -= Time.deltaTime;

		if (mov.AV.btv.shoveItCooldownTimer > 0)
			mov.AV.btv.shoveItCooldownTimer -= Time.deltaTime;

		if (mov.AV.btv.activeKickFlipTokens < mov.AV.btv.kickFlipTokensFromGround + mov.AV.btv.extraKickFlipTokens
			&& mov.GCV.grounded)
			mov.AV.btv.activeKickFlipTokens = mov.AV.btv.kickFlipTokensFromGround + mov.AV.btv.extraKickFlipTokens;

		if (mov.AV.btv.activeShoveItTokens < mov.AV.btv.shoveItTokensFromGround + mov.AV.btv.extraShoveItTokens
			&& mov.GCV.grounded)
			mov.AV.btv.activeShoveItTokens = mov.AV.btv.shoveItTokensFromGround + mov.AV.btv.extraShoveItTokens;
	}

	public override void UpdateAnimator()
	{
		if(mov.AV.btv.kickFlipAnimation)
		{
			mov.AV.btv.kickFlipAnimation = false;
			mov.PlayerAnimator.SetTrigger("SurfJump");
		}

		if (mov.AV.btv.shoveItFlipAnimation)
		{
			mov.AV.btv.shoveItFlipAnimation = false;
			mov.PlayerAnimator.SetTrigger("SurfSwipe");
		}
	}
}
