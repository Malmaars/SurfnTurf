
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class BoudingTeleporter : MonoBehaviour
{
    private Rigidbody playerRB; // Reference to the player's Transform
    [SerializeField] private float teleportThreshold = 50f; // Adjust this value based on your map size
    [SerializeField] private float teleportOffset = 0.9f; // Percentage of the threshold to teleport closer to the center
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
        /*
        if (CloudSaveSystem.Instance.data.playerPosition == Vector3.zero)
        {
            //set player in tutorial area
            CloudSaveSystem.Instance.data.playerPosition = tutorialPoint.position; // Set the initial player position
        }
        */
        playerRB.position = CloudSaveSystem.Instance.data.playerPosition; // Set the player position to the saved position
        playerRB.position = respawnPoint.position; // Set the player position to the saved position
    }
    // Update is called once per frame
    void Update()
    {
        if (playerRB == null) return;

        // Check if the player has moved far enough from the center of the map
        if (Vector3.Distance(playerRB.position, transform.position) > teleportThreshold)
        {
            playerRB.position = RelocatePlayer();
        }
        if (playerRB.position.y < -1)
        {
            RespawnPlayer();
        }
        if (CloudSaveSystem.Instance.IsInitialized)
        {
            CloudSaveSystem.Instance.data.playerPosition = playerRB.position;
        }
    }
    public Vector3 RelocatePlayer()
    {
        if (playerRB == null) return Vector3.zero;
        Vector3 playerPos = playerRB.position;
        if (!IsVectorValid(playerPos))
        {
            Debug.LogWarning("Player position is invalid, respawning instead.");
            RespawnPlayer();
            return respawnPoint != null ? respawnPoint.position : transform.position;
        }

        Vector3 invertedPosition = transform.position - (playerPos - transform.position) * teleportOffset;
        invertedPosition.y = playerPos.y; // Keep the Y position unchanged

        if (!IsVectorValid(invertedPosition))
        {
            Debug.LogWarning("Calculated relocate position is invalid, using respawn or center.");
            return respawnPoint != null ? respawnPoint.position : transform.position;
        }

        return invertedPosition;
    }
    private bool IsVectorValid(Vector3 v)
    {
        return !(float.IsNaN(v.x) || float.IsNaN(v.y) || float.IsNaN(v.z) ||
                 float.IsInfinity(v.x) || float.IsInfinity(v.y) || float.IsInfinity(v.z));
    }
    [Command(nameof(RespawnPlayer))]
    public static void RespawnPlayer()
    {
        //if (respawnPoint == null) return; // Check if respawn point is set
        if (instance != null)
        {
            instance.HeightRespawn();
        }
    }
    public void HeightRespawn()
    {
            playerRB.position = new Vector3(playerRB.position.x, 30, playerRB.position.z);
        
    }

    public void OnDrawGizmos()
    {
        // Draw a wire sphere to represent the teleportation threshold
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, teleportThreshold);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, teleportThreshold / teleportOffset);
        //Gizmos.color = Color.red;
        //Gizmos.DrawSphere(RelocatePlayer(), 50);
    }
}