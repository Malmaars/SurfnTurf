using UnityEngine;

public class ConveyerPlatform : MonoBehaviour
{
    [Header("Platform Settings")]
    public Vector3 moveDirection = Vector3.right;

    public float moveSpeed = 2f;

    private Rigidbody playerRb;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerRb = collision.gameObject.GetComponent<Rigidbody>();
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            playerRb.linearVelocity += moveDirection.normalized * moveSpeed;
            playerRb = null;
        }
    }

    private void LateUpdate()
    {
        if (playerRb != null)
        {
            Vector3 delta = moveDirection.normalized * moveSpeed * Time.deltaTime;
            playerRb.MovePosition(playerRb.position + delta);
        }
    }
}