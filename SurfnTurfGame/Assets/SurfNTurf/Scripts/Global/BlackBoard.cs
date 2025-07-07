using System.Collections.Generic;
using UnityEngine;

public static class BlackBoard 
{
	public static PlayerManager playerManager;
	public static CameraController cameraController;
	public static PlayerVFX playerVFX;
	public static Transform playerBody;
	public static Compass compass;

	public static Dictionary<string, Quest> myquests;
}
