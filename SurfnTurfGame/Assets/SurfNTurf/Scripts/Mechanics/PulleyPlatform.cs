using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PulleyPlatform : MonoBehaviour
{
    private bool idle;
    private Rigidbody playerRb;

    [Header("Platform Settings")]
    public float downwardsAmount;
    public float downwardsSpeed;
    public float upwardsSpeed;
    private float startingHeight;

    private void Start()
    {
        startingHeight = transform.localPosition.y;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.tag == "Player")
        {
            idle = false;
            playerRb = collision.transform.GetComponent<Rigidbody>();
        }
        else
            return;
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.tag == "Player")
        {
            playerRb = null;
        }
    }

    private void LateUpdate()
    {
        if (idle)
            return;
        if (playerRb)
        {
            if (transform.localPosition.y <= startingHeight - downwardsAmount)
                return;
            else
            {
                Vector3 moveAmount = new Vector3(0, downwardsSpeed * Time.deltaTime, 0);
                transform.localPosition -= moveAmount;
                playerRb.MovePosition(playerRb.position - moveAmount);
            }
        }
        else
        {
            if (transform.position.y >= startingHeight)
            {
                idle = true;
                transform.localPosition = new Vector3(transform.localPosition.x, startingHeight, transform.localPosition.z);
            }
            else
                transform.localPosition += new Vector3(0, upwardsSpeed * Time.deltaTime, 0);
        }
    }
}
