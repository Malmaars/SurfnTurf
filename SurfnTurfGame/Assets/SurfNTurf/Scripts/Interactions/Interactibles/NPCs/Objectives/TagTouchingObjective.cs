using UnityEngine;

public class TagTouchingObjective : Objective
{
	public CellTag selectedTagType;
	public CellTag touchingTagType;
	public float percentage;
	public bool completeIfLess;

	public override void RunQuestCheck(NPC _npc)
	{
        if (completeIfLess)
        {
			if (_npc.currentDish.GetNeighbouringTagPercentage(selectedTagType, touchingTagType) <= percentage)
				completed = true;
			else
				completed = false;
		}
        else
        {
			if (_npc.currentDish.GetNeighbouringTagPercentage(selectedTagType, touchingTagType) >= percentage)
				completed = true;
			else
				completed = false;
		}
	}
}
