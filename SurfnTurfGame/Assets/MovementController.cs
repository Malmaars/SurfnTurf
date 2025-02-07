using System;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class MovementController : MonoBehaviour
{
    Rigidbody rb;

    bool grounded;


    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f;

    [SerializeField, Range(0f, 10f)]
    float jumpHeight = 2f;

    Vector3 velocity, desiredVelocity;

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
    public float jumpCancelStrength;
    public float jumpTimer;
    float jumpTimer_countdown;
    bool jumping;

    public bool springy;
    public float pushUpStrength;
    public float heightSpringStrength;
    public float heightSpringDamper;


    private void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();

        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
        InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Jump, EndJump);
    }
    void Update()
    {
        CheckGrounded();

        LandingBehaviour();
        //JumpingBehaviour();
        AddGravity();

        Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        velocity = rb.linearVelocity;
        float maxSpeedChange = maxAcceleration * Time.deltaTime;
        velocity.x =
            Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z =
            Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        if (jumping)
        {
            jumping = false;
            Jump();
        }

        rb.linearVelocity = velocity;
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
            if (hit.collider != null && hit.distance <= desiredHeight)
                grounded = true;
            else
                grounded = false;
        }

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

    void Jump()
    {
        velocity += new Vector3(0, 5f, 0);
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
        jumpTimer_countdown = jumpTimer;
        Debug.Log("JUMP");
    }

    public void EndJump(InputAction.CallbackContext context)
    {
        jumping = false;
    }
}
