using System;
using UnityEngine;
[Serializable]
public class PointObjective : Objective
{
	public int wantedPoints;

	public override void RunQuestCheck()
	{
		if (BlackBoard.cookingManager.plate.GetTotalScore() >= wantedPoints)
			completed = true;
		else
			completed = false;
	}
}
