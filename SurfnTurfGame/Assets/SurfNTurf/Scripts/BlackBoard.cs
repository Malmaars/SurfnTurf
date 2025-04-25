using System.Collections.Generic;
using UnityEngine;

public static class BlackBoard 
{
	public static PlayerManager playerManager;
	public static CameraController cameraController;
	public static CookingDatabase cookingDatabase;
	public static PlayerVFX playerVFX;

	public static Dictionary<string, Quest> myquests;
	public static float UpdateTimer(float _timer)
	{
		if (_timer > 0)
			return _timer - Time.deltaTime;
		return _timer;
	}
}
