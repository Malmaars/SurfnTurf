using System.Collections.Generic;
using UnityEngine;

public static class BlackBoard 
{
	public static PlayerManager playerManager;
	public static CameraController cameraController;
	public static CookingDatabase cookingDatabase;
	public static CookingManager cookingManager;
	public static ChallengeManager challengeManager;
	public static PlayerVFX playerVFX;
	public static Transform playerBody;

	public static Dictionary<string, Quest> myquests;
}
