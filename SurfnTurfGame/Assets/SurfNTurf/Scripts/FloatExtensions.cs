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
	}
}