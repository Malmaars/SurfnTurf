using UnityEngine;

public class TagTouchingObjective : Objective
{
	public CellTag selectedTagType;
	public CellTag touchingTagType;
	public float percentage;
	public bool completeIfLess;

	public override void RunQuestCheck()
	{
        if (completeIfLess)
        {
			if (BlackBoard.cookingManager.plate.GetNeighbouringTagPercentage(selectedTagType, touchingTagType) <= percentage)
				completed = true;
			else
				completed = false;
		}
        else
        {
			if (BlackBoard.cookingManager.plate.GetNeighbouringTagPercentage(selectedTagType, touchingTagType) >= percentage)
				completed = true;
			else
				completed = false;
		}
	}
}
