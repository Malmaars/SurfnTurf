using UnityEngine;

namespace SurfnTurf
{
	public static class FloatExtensions
	{
		public static float TimerCountdown(this float value)
		{
			if (value <= 0)
				return 0;
			return value - Time.deltaTime;
		}

		public static string SecondsToTime(this float value)
		{
			int milliseconds = (int)((value - Mathf.Floor(value)) * 100);
			int seconds = (int)value % 60;
			int minutes = ((int)value / 60) % 60;
			int hours = (int)value / 3600;
			return ""+hours.ToString()+":"+minutes.ToString()+":"+seconds.ToString()+":"+milliseconds.ToString();
		}
	}
}