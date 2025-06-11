using System;
using UnityEngine;
using UnityEngine.Windows;

[Serializable]
public class Quest
{
	public string questName;

	public bool finished;

	[SerializeReference]
	public Objective[] objectives;

	public bool CheckIfFinished()
	{
		finished = true;
		foreach(Objective o in objectives)
		{
			if (!o.CheckIfComplete())
			{
				finished = false;
				break;
			}
		}

		if (!finished)
		{
			foreach (Objective o in objectives)
			{
				o.completed = false;
			}
		}

		return finished;
	}

	public void CheckQuest(NPC _npc)
	{
		foreach(Objective o in objectives)
		{
			o.RunQuestCheck();
		}
	}
}