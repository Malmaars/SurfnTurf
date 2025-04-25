using System;
using UnityEngine;
[Serializable]
public class PointObjective : Objective
{
	public int wantedPoints;

	public override void RunQuestCheck(NPC _npc)
	{
		if (_npc.currentDish.GetTotalScore() >= wantedPoints)
			completed = true;
		else
			completed = false;
	}
}
