using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class Canon : MonoBehaviour
{
    private Rigidbody playerRb;
    [SerializeField] private float launchSpeed = 10f;
    public float anticipationT = 0.5f;
    private float cooldownT = 0.5f;
    private bool launched = false;
    public float gizmoTime = 5f;
    public int gizmoSteps = 20;
    public float gravityModifier = 0.55f;
    [SerializeField] private Transform shootPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (launched) return;
        if (other.CompareTag("Player"))
        {
            playerRb = other.GetComponent<Rigidbody>();
            StartCoroutine(LaunchPlayer());
            launched = true;
        }
    }
    private IEnumerator LaunchPlayer()
    {
        yield return new WaitForSeconds(anticipationT);

        //launch the player with an impusle of force
        Vector3 launchDirection = shootPoint.up * launchSpeed;
        playerRb.linearVelocity = launchDirection;
        yield return new WaitForSeconds(0.2f);
        playerRb.GetComponent<MovementController>().cv.isLaunching = true;

        yield return new WaitForSeconds(cooldownT);
        launched = false;
    }

    //draw gizmo for estimated trejectory
    private void OnDrawGizmos()
    {
        if (shootPoint == null) return;
        Vector3 launchDirection = shootPoint.up * launchSpeed;
        Vector3 startPosition = shootPoint.position;
        Vector3 gravity = Physics.gravity *gravityModifier ;

        Vector3 previousPosition = startPosition;
        for (int i = 1; i <= gizmoSteps; i++)
        {
            float t = (i / (float)gizmoSteps) * gizmoTime;
            Vector3 displacement = launchDirection * t + 0.5f * gravity * t * t;
            Vector3 drawPoint = startPosition + displacement;
            Gizmos.DrawLine(previousPosition, drawPoint);
            previousPosition = drawPoint;
        }
    }

}
