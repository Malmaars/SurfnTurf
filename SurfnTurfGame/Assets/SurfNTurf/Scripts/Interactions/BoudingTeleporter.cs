
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class BoudingTeleporter : MonoBehaviour
{
    [SerializeField] private Rigidbody playerRB; // Reference to the player's Transform
    [SerializeField] private float teleportThreshold = 50f; // Adjust this value based on your map size
    [SerializeField] private float teleportOffset = 0.9f; // Percentage of the threshold to teleport closer to the center
    [SerializeField] private CinemachineCamera cinemachineCamera; // Reference to the player GameObject
    public Transform respawnPoint; // Reference to the respawn point in the scene

    public Transform tutorialPoint;
    //singleton instance
    public static BoudingTeleporter instance;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    IEnumerator Start()
    {
        yield return new WaitUntil(() => CloudSaveSystem.Instance != null && CloudSaveSystem.Instance.IsInitialized);
        playerRB = BlackBoard.playerBody.GetComponent<Rigidbody>(); // Find the player transform in the scene
        if (CloudSaveSystem.Instance.data.playerPosition == Vector3.zero)
        {
            //set player in tutorial area
            CloudSaveSystem.Instance.data.playerPosition = tutorialPoint.position; // Set the initial player position
        }    
            playerRB.position = CloudSaveSystem.Instance.data.playerPosition; // Set the player position to the saved position
    }
    // Update is called once per frame
    void Update()
    {
        if (playerRB == null) return;

        // Check if the player has moved far enough from the center of the map
        if (Vector3.Distance(playerRB.position, transform.position) > teleportThreshold)
        {
            // Teleport the player to the inverted side of the map, closer to the center
            Vector3 offset = playerRB.position - cinemachineCamera.transform.position;
            Quaternion camRotation = cinemachineCamera.transform.rotation;
            Debug.Log(offset);
            Vector3 invertedPosition = transform.position - (playerRB.position - transform.position) * teleportOffset;
            invertedPosition.y = playerRB.position.y; // Keep the Y position unchanged
            playerRB.position = invertedPosition;
            cinemachineCamera.ForceCameraPosition(playerRB.position + offset, camRotation);
        }
        if (playerRB.position.y < -100f)
        {
            RespawnPlayer();
        }
        if (CloudSaveSystem.Instance.IsInitialized)
        {
            CloudSaveSystem.Instance.data.playerPosition = playerRB.position;
        }
    }
    public void RespawnPlayer()
    {
        if (respawnPoint == null) return; // Check if respawn point is set
        playerRB.position = respawnPoint.position;
    }

    void OnDrawGizmosSelected()
    {
        // Draw a wire sphere to represent the teleportation threshold
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, teleportThreshold);
    }
}