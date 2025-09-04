using UnityEngine;
using UnityEngine.InputSystem;

public class Grab : MonoBehaviour
{
    private Rigidbody rb;
    public Transform holdPosition;
    private MovementController mov;
    private Coroutine moveCoroutine;
    private void Start()
    {
        mov = BlackBoard.playerBody.GetComponent<MovementController>();
    }
    public void GrabObject(InputAction.CallbackContext context)
    {
        if (rb != null)
        {
            ReleaseObject();
            return;
        }
        //sphere cast to check for grabbable objects in front of the player
        Collider[] collidersClose = Physics.OverlapSphere(transform.position, 2);
        foreach (Collider col in collidersClose)
        {
            Debug.Log("Check");
            if (!col.CompareTag("Player") && col.CompareTag("Grabbable"))
            {
                if (col.GetComponent<Rigidbody>())
                {
                    Debug.Log("Grab");
                    rb = col.GetComponent<Rigidbody>();
                    rb.isKinematic = true;
                    col.enabled = false;
                    moveCoroutine = StartCoroutine(MoveObjectToHoldPosition(col.gameObject));
                    break;
                }
            }
        }
    }
    //a coroutine for moveing the object to the hold position
    private System.Collections.IEnumerator MoveObjectToHoldPosition(GameObject col)
    {
        float time = 0;
        Vector3 startPos = rb.transform.position;
        Quaternion startRot = rb.transform.rotation;
        while (time < 1)
        {
            time += Time.deltaTime * 5;
            rb.transform.position = Vector3.Lerp(startPos, holdPosition.position, time);
            rb.transform.rotation = Quaternion.Slerp(startRot, holdPosition.rotation, time);
            yield return null;
        }
        rb.transform.position = holdPosition.position;
        rb.transform.rotation = holdPosition.rotation;
        col.transform.SetParent(holdPosition);
    }
    public void ReleaseObject(float force = 5000)
    {
        if (rb != null)
        {
            StopCoroutine(moveCoroutine);
            rb.isKinematic = false;
            rb.transform.SetParent(null);
            rb.AddForce((mov.playerVisual.transform.forward + Vector3.up) * 5000);
            rb.GetComponent<Collider>().enabled = true;
            rb = null;
            Debug.Log("Release");
        }
    }
}
