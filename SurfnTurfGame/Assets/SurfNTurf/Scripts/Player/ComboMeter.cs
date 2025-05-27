using SurfnTurf;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public delegate void OnAddCombo();
public static class ComboMeter
{
	public static int currentCombo { get; private set; }
	public static float comboCoolDownTimer { get; private set; }
	public static float comboCoolDown;

	public static event OnAddCombo onAddCombo;
	public static string lastMove { get; private set; }

	public static void AddToCombo(string comboMove)
	{
		if (lastMove == comboMove)
		{
			//reset the counter;
			currentCombo = 1;
		}
		else
		{
			currentCombo++;
		}
		comboCoolDownTimer = comboCoolDown;
		SetName(comboMove);
		onAddCombo.Invoke();
	}

	static void SetName(string _name)
	{
		lastMove = _name;

		//add it to the visual or something
	}

	public static void OnUpdate()
	{
		comboCoolDownTimer = comboCoolDownTimer.TimerCountdown();

		if(comboCoolDownTimer <= 0)
		{
			currentCombo = 0;
		}
	}
}
