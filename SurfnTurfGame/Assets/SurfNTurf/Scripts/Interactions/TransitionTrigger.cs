using Unity.VisualScripting;
using UnityEngine;

public class TransitionTrigger : MonoBehaviour
{
    [SerializeField] private GameObject[] CurrentSceneObjects;
    [SerializeField] private GameObject[] NextSceneObjects;
    [SerializeField] private Transform playerSpawnPoint;


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SceneTranstitionState.instance.CurrentSceneObjects = CurrentSceneObjects;
            SceneTranstitionState.instance.NextSceneObjects = NextSceneObjects;
            SceneTranstitionState.instance.playerSpawnPoint = playerSpawnPoint;
            BoudingTeleporter.instance.respawnPoint = playerSpawnPoint;
            BlackBoard.playerManager.SwitchState(typeof(SceneTranstitionState));
        }
    }
}

