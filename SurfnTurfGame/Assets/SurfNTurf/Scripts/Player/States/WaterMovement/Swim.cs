using UnityEngine;

public class Swim : WaterAbility
{
    public Swim(WaterMovementController _mov) : base(_mov)
	{
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSwim();
	}

	void HandleSwim()
	{
		if (mov.wv.onWater)
			mov.sv.swimming = true;

		else
			mov.sv.swimming = false;

		if (mov.sv.swimming)
		{

		}
	}
}
