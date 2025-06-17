 using UnityEngine;
using System;
using System.Reflection;

[Serializable]
public abstract class Objective
{
	public bool completed;
	public virtual bool CheckIfComplete()
	{
		return completed;
	}

	public virtual void RunQuestCheck()
	{
		return;
	}

	public virtual int GetValueAmount()
    {
		return 0;
    }
}
