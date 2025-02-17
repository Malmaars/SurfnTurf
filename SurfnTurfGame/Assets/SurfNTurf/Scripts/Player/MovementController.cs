using System;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public class MovementController : MonoBehaviour
{
    Rigidbody rb;
    Vector3 velocity, desiredVelocity;

    public Transform playerVisual;
    public Animator animator;

    [SerializeField, Range(0f, 100f)]
    float visualRotationSpeed = 10f;

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

    [SerializeField, Range(0f, 1f)]
    float minGroundDotProduct;

    Vector3 contactNormal;

    public MoveType moveType;

    [SerializeField, Range(0f, 50f)]
    public float LerpStrength;

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

    [SerializeField, Range(0f, 10f)]
    float forwardRaysDistance = 2f;

    [SerializeField, Range(0f, 3f)]
    float maxSlopeHeight = 0.5f;



    [Header("Air control")]

    [SerializeField, Range(0f, 100f)]
    float maxAirAcceleration = 10f;


    [SerializeField, Range(-100f, 0f)]
    public float customGravityStrength;
    float currentGravityStrength;

    [SerializeField, Range(-200f, 0f)]
    public float maximumDownVelocity;

    float antiAirTimer;


    [Header("Jumping")]

    [SerializeField, Range(0f, 100f)]
    float jumpHeight = 2f;

    [SerializeField, Range(0, 5)]
    int maxAirJumps = 0;
    int jumpPhase;

    Vector3 jumpDirection;

    public float jumpTimer;
    float jumpTimer_countdown;
    bool desiredJump;
    bool jumping;
    bool hasLanded;
    bool hasLandedAnimation;

    [Header("Wall Jumping")]
    [SerializeField, Range(4, 64)]
    int wallRaycastAmount = 4;
    [SerializeField, Range(0, 4)]
    float distanceUntilWallGrab = 1;
    [SerializeField, Range(0, 1)]
    float maxWallAngleOffsetZeroToOne;

    [SerializeField, Range(0, 1)]
    float inputDirectionLeeway = 0.2f;

    [SerializeField, Range(-30, 0)]
    float wallGrabGravity = -3f;

    [SerializeField, Range(0f, 100f)]
    float wallJumpForce = 2f;

    [SerializeField, Range(0f, 100f)]
    float wallRidingJumpForce = 2f;


    [SerializeField, Range(0f, 90f)]
    float walljumpAngle = 45f;

    [SerializeField, Range(0f, 10f)]
    float wallJumpAntiAirTimer = 0.5f;


    bool wallgrab;
    bool wallRiding;
    bool wallJumped;


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
            Vector3 gradient = new Vector3(0, 0, 0);
            gradient.x = contactNormal.x * contactNormal.z;
            gradient.y = contactNormal.y * contactNormal.z;
            gradient.z = -(contactNormal.x * contactNormal.x) - (contactNormal.y * contactNormal.y);
            gradient = Vector3.ProjectOnPlane(contactNormal, Vector3.up).normalized;
            Gizmos.DrawLine(rb.position, rb.position + gradient.normalized * desiredHeight);

            Gizmos.color = Color.red;
            float angleStep = 360f;
            for (float i = 0; i < wallRaycastAmount; i ++)
            {
                // Calculate the angle for the current raycast
                float angle = 90 + i * (angleStep / wallRaycastAmount);

                // Convert the angle to radians, then create a direction vector using cosine and sine for the x and z axes
                Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle));

                Gizmos.DrawLine(rb.position, rb.position + direction * distanceUntilWallGrab);
            }

                Vector3 startPos = rb.position + playerVisual.forward.normalized * forwardRaysDistance;
                Gizmos.DrawLine(startPos, startPos + Vector3.down * maxSlopeHeight);
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


        velocity = rb.linearVelocity;
        
        newMovementVector = newMovementVector.normalized;
        desiredVelocity = newMovementVector * maxSpeed;

        float acceleration = grounded ? maxAcceleration : maxAirAcceleration;
        if (grounded && onSlope)
            acceleration = maxSlopeAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        if (grounded)
        {
            antiAirTimer = 0;
            wallJumped = false;
            wallRiding = false;

            if (!onSlope && !jumping)
            {
                velocity.y = 0;
            }
        }

        if (wallJumped && antiAirTimer <= 0 && playerInput != Vector2.zero)
            wallJumped = false;

        if (antiAirTimer <= 0 && !(wallJumped && playerInput == Vector2.zero))
        {
            velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);
        }

        CheckForWalls();
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

        CheckLanding();

        if (new Vector3(velocity.x, 0, velocity.z).sqrMagnitude > 0.01f && new Vector3(velocity.x, 0, velocity.z) != Vector3.zero && playerVisual.forward != new Vector3(velocity.x, 0, velocity.z))
        {
            Quaternion newRotation = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));

            playerVisual.localRotation = Quaternion.Slerp
               (playerVisual.localRotation, newRotation, visualRotationSpeed * Time.deltaTime);
        }

        if (antiAirTimer > 0)
            antiAirTimer -= Time.deltaTime;

        rb.linearVelocity = velocity;

        UpdateAnimator();
    }
    void CheckGrounded()
    {
        RaycastHit hit;

        Physics.Raycast(rb.position, Vector3.down, out hit, rayLength);

        //send multiple raycasts in multiple direction to check with part of the ground is the closest


        //send a raycast down to check for ground, if player is grounded, keep it at a certain height from the ground

        if (grounded && hit.normal.y >= minGroundDotProduct)
        {
            Physics.Raycast(rb.position, -hit.normal.normalized, out hit, rayLength);
        }

        bool forwardGrounded = false;
        if (grounded)
        {
            Vector3 startPos = rb.position + playerVisual.forward.normalized * forwardRaysDistance;
            RaycastHit slopeHit;

            Physics.Raycast(startPos, Vector3.down, out slopeHit, maxSlopeHeight);
            if (!jumping && slopeHit.collider != null && hit.distance > desiredHeight)
            {
                forwardGrounded = true;
                grounded = true;
            }
        }

        if (hit.collider != null && hit.distance <= groundedDistance)
            grounded = true;
        else if (!forwardGrounded)
            grounded = false;

        if (grounded)
        {
            Vector3 normal = hit.normal;
            if (normal.y < contactNormal.y && normal.y == 0)
                hasLanded = false;
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

        switch (moveType)
        {
            case MoveType.springy:
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
                break;
            case MoveType.snap:
                if(!onSlope)
                    rb.position = (hit.point + Vector3.up * desiredHeight);
                break;
            case MoveType.lerp:
                rb.position = Vector3.Lerp(rb.position, (hit.point + Vector3.up * desiredHeight), LerpStrength * Time.deltaTime);
                break;
            case MoveType.linearLerp:
                float currentyPos = hit.point.y - rb.position.y;
                rb.position = rb.position + ((hit.point + Vector3.up * desiredHeight) - rb.position) * LerpStrength * Time.deltaTime;

                if((currentyPos < 0 && hit.point.y - rb.position.y >= 0) ||
                    (currentyPos > 0 && hit.point.y - rb.position.y <= 0))
                    rb.position = (hit.point + Vector3.up * desiredHeight);
                break;
        }
    }

    void Jump()
    {
        if (grounded || jumpPhase <= maxAirJumps)
        {

            jumping = true;
            jumpTimer_countdown = jumpTimer;

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

            if (!wallgrab)
            {
                velocity += contactNormal * jumpSpeed;
                if (jumpSpeed > 0f)
                    jumpPhase++;
            }

            wallJumped = false;
        }
        else if(wallgrab || wallRiding)
        {
            jumping = true;
            jumpTimer_countdown = jumpTimer;

            Vector3 newDir = (jumpDirection + Vector3.up);
            newDir.Normalize();
            newDir = new Vector3(newDir.x, Mathf.Tan(Mathf.Deg2Rad * walljumpAngle), newDir.z);

            velocity = Vector3.zero;
            velocity += newDir * wallJumpForce;
            antiAirTimer = wallJumpAntiAirTimer;
            wallJumped = true;
            wallgrab = false;
            wallRiding = false;
            jumpPhase = 1;
        }
    }

    void AddGravity()
    {
        if (grounded == true)
            return;

        if (rb.linearVelocity.y > maximumDownVelocity && !onSlope)
        {
            //apply a consistent downforce, perhaps greater than normal gravity
            if (!wallgrab)
                rb.AddForce(Vector3.up * customGravityStrength);
            else
                velocity.y = +wallGrabGravity;
        }
    }

    void CheckLanding()
    {

        if (grounded && !jumping && !onSlope && !hasLanded)
        {
            velocity.y = 0;
            hasLanded = true;
            hasLandedAnimation = true;
        }
        if (!grounded)
            hasLanded = false;
    }

    void CheckForWalls()
    {
        //send out a couple raycasts in multiple directions
        float angleStep = 360f;
        for (float i = 0; i < wallRaycastAmount; i++)
        {
            // Calculate the angle for the current raycast
            float angle = 90 + i * (angleStep / wallRaycastAmount);

            // Convert the angle to radians, then create a direction vector using cosine and sine for the x and z axes
            Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle));
            RaycastHit hit;

            wallgrab = false;
            Physics.Raycast(rb.position, direction, out hit, distanceUntilWallGrab);
            if (hit.collider != null && hit.normal.y >= 0f - maxWallAngleOffsetZeroToOne && hit.normal.y <= 0f + maxWallAngleOffsetZeroToOne)
            {
                //we're up against a wall

                if (Vector3.Dot(new Vector3(velocity.x, 0, velocity.z).normalized, direction) >= 1 - inputDirectionLeeway)
                {
                    if (velocity.y < 0f)
                    {
                        velocity.x = 0f;
                        velocity.z = 0f;
                        //player is aiming at the wall
                        wallgrab = true;
                        jumpDirection = (hit.normal + Vector3.up) / 2;
                        break;
                    }
                }

                else if (Vector3.Dot(new Vector3(velocity.x, 0, velocity.z).normalized, hit.point - rb.position) > 0)
                {
                    //we are touching a wallF just not hugging it
                    jumpDirection = ((hit.normal * 1.2f + Vector3.up + velocity.normalized) / 3);
                    wallRiding = true;
                    wallgrab = false;
                }
            }
        }

        //if the player is leaning against a wall, make slow them down;


    }

    void AddSlope()
    {
        if (!onSlope)
            return;
        Vector3 gradient = new Vector3(0, 0, 0);
        gradient.x = contactNormal.x * contactNormal.z;
        gradient.y = contactNormal.y * contactNormal.z;
        gradient.z = -(contactNormal.x * contactNormal.x) - (contactNormal.y * contactNormal.y);

        gradient = Vector3.ProjectOnPlane(contactNormal, Vector3.up).normalized;
        rb.AddForce(gradient.normalized * slopeGlideStrength);
    }

    public void StartJump(InputAction.CallbackContext context)
    {
        desiredJump = true;

    }

    public void EndJump(InputAction.CallbackContext context)
    {
        desiredJump = false;
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", new Vector2(velocity.x, velocity.z).magnitude / 10);

        if (jumping != animator.GetBool("Jumping"))
            animator.SetBool("Jumping", jumping);

        if (((!grounded && !jumping) || onSlope) != animator.GetBool("Falling"))
            animator.SetBool("Falling", !grounded && !jumping);

        if (grounded == true)
        {
            if (hasLandedAnimation == true)
            {
                animator.SetTrigger("Landing");
                hasLandedAnimation = false;
            }
        }
        else
            hasLandedAnimation = false;
    }
}
