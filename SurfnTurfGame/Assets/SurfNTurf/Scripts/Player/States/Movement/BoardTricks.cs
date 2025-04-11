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
	}
}
