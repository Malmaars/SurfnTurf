using UnityEngine;

public class TagAmountObjective : Objective
{
	public CellTag tagType;
	public float percentage;
	public override void RunQuestCheck(NPC _npc)
	{
		if (_npc.currentDish.GetTagPercentage(tagType) >= percentage)
			completed = true;
		else
			completed = false;
	}
}
