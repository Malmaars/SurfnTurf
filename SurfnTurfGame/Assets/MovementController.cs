using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class MovementController : MonoBehaviour
{
    Rigidbody rb;
    Vector3 velocity, desiredVelocity;

    [Header("Ground control")]

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 100f)]
    float maxAcceleration = 10f;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f;

    public LayerMask groundedLayerMask;
    bool grounded;

    //This is the length of the raycast, it should be a little higher than the desired height
    public float rayLength;

    //The height we want the player to be from the ground.
    public float desiredHeight;

    public bool springy;
    public float pushUpStrength;
    public float heightSpringStrength;
    public float heightSpringDamper;

    [Header("Air control")]

    [SerializeField, Range(0f, 100f)]
    float maxAirAcceleration = 10f;


    [SerializeField, Range(-100f, 0f)]
    public float customGravityStrength;

    public float maximumDownVelocity;


    [Header("Jumping")]

    [SerializeField, Range(0f, 100f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;
    int jumpPhase;

    public float jumpTimer;
    float jumpTimer_countdown;
    bool jumping;


    private void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();

        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
        InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Jump, EndJump);
    }
    void Update()
    {
        CheckGrounded();


        Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        desiredVelocity = new Vector3(playerInput.x, 0f, playerInput.y) * maxSpeed;

        velocity = rb.linearVelocity;
        float acceleration = grounded ? maxAcceleration : maxAirAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        velocity.x =
            Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z =
            Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);


        AddGravity();
        LandingBehaviour();
        if(grounded)
            jumpPhase = 0;


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
            velocity = new Vector3(velocity.x, 0, velocity.z);
        }

    }

    void Jump()
    {
        float jumpSpeed = jumpHeight;

        if (velocity.y > 0f)
        {
            jumpSpeed = jumpSpeed - velocity.y;
        }

        jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);

        velocity += new Vector3(0, jumpSpeed, 0);

        if (jumpSpeed > 0f)
            jumpPhase++;

    }

    void AddGravity()
    {
        if (grounded == true)
            return;

        if (rb.linearVelocity.y > maximumDownVelocity)
        {
            //apply a consistent downforce, perhaps greater than normal gravity
            rb.AddForce(Vector3.up * customGravityStrength);
        }
    }

    public void StartJump(InputAction.CallbackContext context)
    {
        if (jumpPhase > maxAirJumps && (!grounded || jumping))
            return;

        jumping = true;
        jumpTimer_countdown = jumpTimer;
    }

    public void EndJump(InputAction.CallbackContext context)
    {
        jumping = false;
    }
}
