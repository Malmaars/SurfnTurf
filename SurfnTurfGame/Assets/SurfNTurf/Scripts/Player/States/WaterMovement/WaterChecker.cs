using UnityEngine;

public class WaterChecker : WaterAbility
{
	public WaterChecker(WaterMovementController _mov) : base(_mov) { }

	public override void RunOnUpdateBeforeSetVelocity()
	{
		CheckIfOnWater();
	}

	void CheckIfOnWater()
	{
		if (mov.wv.waterContactCount > 0)
			mov.wv.onWater = true;
		else
			mov.wv.onWater = false;
	}
}
