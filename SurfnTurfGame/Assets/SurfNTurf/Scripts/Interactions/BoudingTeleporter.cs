
using Unity.Cinemachine;
using UnityEngine;

public class BoudingTeleporter : MonoBehaviour
{
    [SerializeField] private Transform playerTransform; // Reference to the player's Transform
    [SerializeField] private float teleportThreshold = 50f; // Adjust this value based on your map size
    [SerializeField] private float teleportOffset = 0.9f; // Percentage of the threshold to teleport closer to the center
    [SerializeField] private CinemachineCamera cinemachineCamera; // Reference to the player GameObject

    // Update is called once per frame
    void Update()
    {
        if (playerTransform == null) return;

        // Check if the player has moved far enough from the center of the map
        if (Vector3.Distance(playerTransform.position, transform.position) > teleportThreshold)
        {
            // Teleport the player to the inverted side of the map, closer to the center
            Vector3 offset = playerTransform.position - cinemachineCamera.transform.position;
            Quaternion camRotation = cinemachineCamera.transform.rotation;
            Debug.Log(offset);
            Vector3 invertedPosition = transform.position - (playerTransform.position - transform.position) * teleportOffset;
            invertedPosition.y = playerTransform.position.y; // Keep the Y position unchanged
            playerTransform.position = invertedPosition;
            cinemachineCamera.ForceCameraPosition(playerTransform.position + offset, camRotation);
        }
    }

    void OnDrawGizmosSelected()
    {
        // Draw a wire sphere to represent the teleportation threshold
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, teleportThreshold);
    }
}