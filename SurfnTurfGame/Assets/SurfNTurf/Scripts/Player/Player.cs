using System.Collections;
using Unity.Cinemachine;
using Unity.Mathematics;
using Unity.VisualScripting;
//using UnityEditorInternal;
using UnityEngine;
using UnityEngine.InputSystem;

public enum JumpType
{
    added,
    explosive
}
public class Player : MonoBehaviour
{
    [Header("Editor components")]
    public Rigidbody rb;
    public CinemachineCamera playerCamera;

    [Header("Movement")]
    bool grounded;
    Vector3 newVelocity;
    public float moveSpeed;
    public float maximumMoveVelocity;

    Vector2 lastMovementVelocity;


    [Header("Air control")]
    public LayerMask groundedLayerMask;

    //This is the length of the raycast, it should be a little higher than the desired height
    public float rayLength;

    //The height we want the player to be from the ground.
    public float desiredHeight;

    public float customGravityStrength;
    public float maximumDownVelocity;


    [Header("Jumping")]
    public JumpType jumpType;


    public float addedjumpStrength;
    public float explosivejumpStrength;
    public float jumpTimer;
    float jumpTimer_countdown;
    bool jumping;

    public bool springy;
    public float pushUpStrength;
    public float heightSpringStrength;
    public float heightSpringDamper;



    void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();

        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
        InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Jump, EndJump);
    }

    void Update()
    {
        newVelocity = rb.linearVelocity;
        CheckGrounded();
        LandingBehaviour();
        JumpingBehaviour();
        UpdateMovement();
        AddGravity();

        rb.linearVelocity = newVelocity;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(rb.position, rb.position + Vector3.down * rayLength);
    }

    void CheckGrounded()
    {
        //send a raycast down to check for ground, if player is grounded, keep it at a certain height from the ground
        RaycastHit hit;

        Physics.Raycast(rb.position, Vector3.down, out hit, rayLength);

        if (springy)
        {
            if (hit.collider != null)
                grounded = true;
            else
                grounded = false;
        }
        else
        {
            if (hit.collider != null && hit.distance < desiredHeight )
                grounded = true;
            else
                grounded = false; 
        }

    }

    void UpdateMovement()
    {
        //check the position of the camera, then calculate the direction based on its angle towards the player
        Vector3 cameraDirection = playerCamera.transform.forward;
        Vector3 cameraRightDirection = playerCamera.transform.right;

        cameraDirection = new Vector3(cameraDirection.x, 0, cameraDirection.z).normalized;
        cameraRightDirection = new Vector3(cameraRightDirection.x, 0, cameraRightDirection.z).normalized;

        Vector2 movementInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
        Debug.Log(movementInput);
        //y is forward/backward from camera view, x is moving to the side
        Vector3 newMovementVector = cameraDirection * movementInput.y * moveSpeed;
        newMovementVector += cameraRightDirection * movementInput.x * moveSpeed;


        //don't just add velocity, track all velocity and add movement when possible

        if (rb.linearVelocity.x + newVelocity.x > maximumMoveVelocity)
            newMovementVector = new Vector3(0, newMovementVector.y, newMovementVector.z);
        if (rb.linearVelocity.z + newVelocity.z > maximumMoveVelocity)
            newMovementVector = new Vector3(newMovementVector.x, newMovementVector.y, 0);
        

        newVelocity += newMovementVector;
    }

    void LandingBehaviour()
    {
        if (grounded == false || jumping == true)
            return;
        //push the player to the desired height
        
        RaycastHit hit;
        Physics.Raycast(rb.position, Vector3.down, out hit, rayLength);

        if (springy)
        {
            Vector3 vel = rb.linearVelocity;
            Vector3 rayDir = Vector3.down;

            Vector3 othervel = Vector3.zero;
            Rigidbody hitbody = hit.rigidbody;

            if (hitbody != null)
            {
                othervel = hitbody.linearVelocity;
            }

            float rayDirVel = Vector3.Dot(rayDir, vel);
            float otherDirVel = Vector3.Dot(rayDir, othervel);

            float relVel = rayDirVel - otherDirVel;

            float x = hit.distance - desiredHeight;

            float springForce = (x * heightSpringStrength) - (relVel * heightSpringDamper);

            rb.AddForce(rayDir * springForce);
        }
        else
        {
            rb.position = new Vector3(rb.position.x, hit.point.y + desiredHeight, rb.position.z);
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }

    }

    void JumpingBehaviour()
    {
        if (jumping == false)
            return;

        if (rb.linearVelocity.y < 0)
        {
            //we're going down, so we're no longer jumping
            jumping = false;
        }
        if (jumpType == JumpType.added)
        {
            rb.AddForce(Vector3.up * addedjumpStrength);
        }
        else if (jumpType == JumpType.explosive && grounded == false)
        {
            rb.AddForce(Vector3.down * customGravityStrength);
        }

        jumpTimer_countdown -= Time.deltaTime;

        if (jumpTimer_countdown <= 0)
        {
            jumping = false;
        }
    }

    void AddGravity()
    {
        if (grounded == true || jumping == true)
            return;

        if (rb.linearVelocity.y > maximumDownVelocity)
        {
            //apply a consistent downforce, perhaps greater than normal gravity
            rb.AddForce(Vector3.down * customGravityStrength);
        }
    }

    public void StartJump(InputAction.CallbackContext context)
    {
        if (grounded == false || jumping == true)
            return;


        jumping = true;

        if(jumpType == JumpType.explosive)
        {
            if(rb.linearVelocity.y < 0)
            {
                rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0);
            }

            //TODO change this from an addforce function to a jump movement made by hand
            rb.AddForce(Vector3.up * explosivejumpStrength, ForceMode.Impulse);
        }

        jumpTimer_countdown = jumpTimer;
        Debug.Log("Jump!");
    }

    public void EndJump(InputAction.CallbackContext context)
    {
        jumping = false;
    }
}
