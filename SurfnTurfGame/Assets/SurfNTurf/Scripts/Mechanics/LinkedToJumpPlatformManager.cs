using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class LinkedToJumpPlatformManager : MonoBehaviour
{

    public List<LinkedToJumpPlatform> platforms;

    private void OnEnable()
    {
        //BlackBoard.playerBody.GetComponent<MovementController>().
        Jump.OnPlayerJump += ActivatePlatforms;
    }

    private void OnDisable()
    {
        Jump.OnPlayerJump -= ActivatePlatforms;
    }
    private void ActivatePlatforms()
    {
        foreach (LinkedToJumpPlatform platform in platforms)
        {
            platform.SwitchPlatform();
        }
    }
}
