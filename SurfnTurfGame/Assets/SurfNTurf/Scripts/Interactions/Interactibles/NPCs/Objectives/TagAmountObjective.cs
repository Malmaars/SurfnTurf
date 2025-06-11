using UnityEngine;

public class TagAmountObjective : Objective
{
	public CellTag tagType;
	public float percentage;
	public override void RunQuestCheck()
	{
		if (BlackBoard.cookingManager.plate.GetTagAmount(tagType) >= percentage)
			completed = true;
		else
			completed = false;
	}
}
