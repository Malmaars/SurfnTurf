using UnityEngine;

public class SpinningPlatform : MonoBehaviour
{
    private Rigidbody playerRb;
    public Transform root;
    public float rotationSpeed = 10f;



    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = other.GetComponent<Rigidbody>();
            //TODO dont grab the player but do it the right way
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerRb = null;
        }
    }

    private void LateUpdate()
    {
        root.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

        if (playerRb != null && root != null)
        {
            Vector3 delta = GetPlatformRotationDelta(root, playerRb.transform);
            playerRb.MovePosition(playerRb.position + delta);
        }
    }

    public Vector3 GetPlatformRotationDelta(Transform platform, Transform player)
    {
        // Calculate the player's position relative to the platform's pivot
        Vector3 relativePos = player.position - platform.position;

        // Calculate the rotation for this frame
        Quaternion rotation = Quaternion.Euler(0, rotationSpeed * Time.deltaTime, 0);

        // Rotate the relative position
        Vector3 rotatedRelativePos = rotation * relativePos;

        // The new world position after rotation
        Vector3 newWorldPos = platform.position + rotatedRelativePos;

        // Delta is the difference between new and old position
        return newWorldPos - player.position;
    }
}
