using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

public class MovementController : MonoBehaviour
{
    Rigidbody rb;
    Vector3 velocity, desiredVelocity;

    public Transform playerVisual;

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
    [SerializeField, Range(0f, 20f)]
    public float rayLength;

    //The height we want the player to be from the ground.
    [SerializeField, Range(0f, 10f)]
    public float groundedDistance;

    [SerializeField, Range(0f, 5f)]
    public float desiredHeight;

    float minGroundDotProduct;
    Vector3 contactNormal;

    public bool springy;
    [SerializeField, Range(0f, 10f)]
    public float pushUpStrength;
    [SerializeField, Range(0f, 100f)]
    public float heightSpringStrength;
    [SerializeField, Range(0f, 10f)]
    public float heightSpringDamper;

    bool onSlope;
    [SerializeField, Range(0f, 100f)]
    public float slopeGlideStrength;
    [SerializeField, Range(0f, 100f)]
    float maxSlopeAcceleration = 1f;


    [Header("Air control")]

    [SerializeField, Range(0f, 100f)]
    float maxAirAcceleration = 10f;


    [SerializeField, Range(-100f, 0f)]
    public float customGravityStrength;

    [SerializeField, Range(-200f, 0f)]
    public float maximumDownVelocity;


    [Header("Jumping")]

    [SerializeField, Range(0f, 100f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;
    int jumpPhase;

    public float jumpTimer;
    float jumpTimer_countdown;
    bool desiredJump;
    bool jumping;

    private void OnValidate()
    {
        minGroundDotProduct = Mathf.Cos(maxGroundAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        OnValidate();

        if (!Application.isEditor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
    private void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();

        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
        InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Jump, EndJump);
    }

    private void OnDrawGizmos()
    {
        if (rb == null)
            rb = GetComponentInChildren<Rigidbody>();

        if (rb != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(rb.position, rb.position + -contactNormal.normalized * desiredHeight);
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(rb.position + -contactNormal.normalized * desiredHeight, rb.position + -contactNormal.normalized * groundedDistance);
            Gizmos.color = Color.green;
            Vector3 gradient = new Vector3(0,0,0);
            gradient.x = contactNormal.x * contactNormal.z;
            gradient.y = contactNormal.y * contactNormal.z;
            gradient.z = -(contactNormal.x * contactNormal.x) - (contactNormal.y * contactNormal.y);
            Gizmos.DrawLine(rb.position, rb.position + gradient.normalized * desiredHeight);
        }
    }
    void Update()
    {
        CheckGrounded();


        Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        Vector3 cameraDirection = Camera.main.transform.forward;
        Vector3 cameraRightDirection = Camera.main.transform.right;
        cameraDirection = new Vector3(cameraDirection.x, 0, cameraDirection.z).normalized;
        cameraRightDirection = new Vector3(cameraRightDirection.x, 0, cameraRightDirection.z).normalized;
        Vector3 newMovementVector = cameraDirection * playerInput.y;
        newMovementVector += cameraRightDirection * playerInput.x;

        newMovementVector = newMovementVector.normalized;
        desiredVelocity = newMovementVector * maxSpeed;

        velocity = rb.linearVelocity;
        float acceleration = grounded ? maxAcceleration : maxAirAcceleration;
        if (grounded && onSlope)
            acceleration = maxSlopeAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;
        velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
        velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

        if (!(velocity.x == 0 && velocity.y == 0 && velocity.z == 0))
            playerVisual.forward = new Vector3(velocity.x, 0, velocity.z);

        AddGravity();
        AddSlope();
        LandingBehaviour();
        if(grounded && !jumping)
            jumpPhase = 0;

        if (desiredJump)
        {
            desiredJump = false;
            Jump();
        }

        if (jumping && velocity.y < 0f)
            jumping = false;

        rb.linearVelocity = velocity;
    }
    void CheckGrounded()
    {
        //send a raycast down to check for ground, if player is grounded, keep it at a certain height from the ground
        RaycastHit hit;

        Physics.Raycast(rb.position, Vector3.down, out hit, rayLength);
        if (grounded && hit.normal.y >= minGroundDotProduct)
        {
            Physics.Raycast(rb.position, -hit.normal.normalized, out hit, rayLength);
        }

        if (springy)
        {
            if (hit.collider != null && hit.distance <= groundedDistance)
                grounded = true;
            else
                grounded = false;
        }
        else
        {
            if (hit.collider != null && hit.distance <= groundedDistance)
                grounded = true;
            else
                grounded = false;
        }

        if (grounded)
        {
            Vector3 normal = hit.normal;
            contactNormal = normal;
            onSlope = normal.y < minGroundDotProduct;
        }
        else
        {
            contactNormal = Vector3.up;
            onSlope = false;
        }
    }

    void LandingBehaviour()
    {
        if (grounded == false || jumping == true || desiredJump)
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
        else if(!onSlope)
        {
            rb.position = (hit.point + Vector3.up * desiredHeight);
        }

    }

    void Jump()
    {
        float jumpSpeed = jumpHeight;
        float alignedSpeed = Vector3.Dot(velocity, contactNormal);

        if (velocity.y > 0f)
        {
            jumpSpeed = jumpSpeed - velocity.y;
        }

        if (alignedSpeed > 0f)
        {
            jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
        }
        velocity += contactNormal * jumpSpeed;

        if (jumpSpeed > 0f)
            jumpPhase++;

    }

    void AddGravity()
    {
        if (grounded == true)
            return;

        if (rb.linearVelocity.y > maximumDownVelocity && !onSlope)
        {
            //apply a consistent downforce, perhaps greater than normal gravity
            rb.AddForce(Vector3.up * customGravityStrength);
        }
    }

    void AddSlope()
    {
        if (!onSlope)
            return;
        Vector3 gradient = new Vector3(0, 0, 0);
        gradient.x = contactNormal.x * contactNormal.z;
        gradient.y = contactNormal.y * contactNormal.z;
        gradient.z = -(contactNormal.x * contactNormal.x) - (contactNormal.y * contactNormal.y);

        rb.AddForce(gradient.normalized * slopeGlideStrength);
    }

    public void StartJump(InputAction.CallbackContext context)
    {
        if (jumpPhase > maxAirJumps && (!grounded || jumping))
            return;

        desiredJump = true;
        jumping = true;
        jumpTimer_countdown = jumpTimer;
    }

    public void EndJump(InputAction.CallbackContext context)
    {
        desiredJump = false;
    }
}
