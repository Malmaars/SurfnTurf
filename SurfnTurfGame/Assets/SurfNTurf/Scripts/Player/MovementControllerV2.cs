using System;
using System.Timers;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;


//Version 2 of the movement controller will be using collissions instead of raycasts to check being grounded
public class MovementControllerV2 : MonoBehaviour
{
    Rigidbody rb;
    Vector3 velocity, desiredVelocity;

    public Transform playerVisual;
    public Animator animator;

    Vector2 lastPlayerInput;

    [SerializeField, Range(0f, 100f)]
    float visualRotationSpeed = 10f;

    [Header("Ground control")]

    [SerializeField, Range(0f, 100f)]
    float maxSpeed = 10f;

    [SerializeField, Range(0f, 500f)]
    float maxAcceleration = 10f;

    [SerializeField, Range(0f, 90f)]
    float maxGroundAngle = 25f;
    float minGroundDotProduct;
    [SerializeField, Range(0f, 90f)]
    float minSlopeAngle = 25f;
    float minSlopeDotProduct;

    public LayerMask groundedLayerMask;

    int groundContactCount;
    bool grounded;
    int stepsSinceLastGrounded;
    [SerializeField, Range(0f, 100f)]
    float maxSnapSpeed = 100f;
    [SerializeField, Min(0f)]
    float groundSnapProbeDistance = 1f;

    Vector3 contactNormal;

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

    [SerializeField, Range(0f, 500f)]
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

    int stepsSinceLastJump;
    public float jumpTimer;
    float jumpTimer_countdown;
    bool desiredJump;
    bool jumping;
    bool hasLanded;
    bool hasLandedAnimation;
    bool inAir;

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
        minSlopeDotProduct = Mathf.Cos(minSlopeAngle * Mathf.Deg2Rad);
    }

    private void Awake()
    {
        OnValidate();

        if (!Application.isEditor)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        Time.timeScale = 1.0f;
    }
    private void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();

        InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
        InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Jump, EndJump);
    }
    void OnCollisionEnter(Collision collision)
    {
        //onGround = true;
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
    {
        //onGround = true;
        EvaluateCollision(collision);
    }
    private void OnCollisionExit(Collision collision)
    {
        //Debug.Log("triggering on collisionexit");
        EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
        onSlope = false;
        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= minGroundDotProduct)
            {
                if (normal.y <= minSlopeDotProduct)
                    onSlope = true;

                groundContactCount++;
                contactNormal += normal;
            }
        }
        if (groundContactCount > 0)
            stepsSinceLastGrounded = 0;
        if (groundContactCount > 1)
            contactNormal.Normalize();
        else if (groundContactCount == 0)
            contactNormal = Vector3.zero;
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - contactNormal * Vector3.Dot(vector, contactNormal);
    }

    void AdjustVelocity()
    {
        Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
        playerInput = Vector2.ClampMagnitude(playerInput, 1f);
        Vector3 cameraDirection = Camera.main.transform.forward;
        Vector3 cameraRightDirection = Camera.main.transform.right;
        Vector3 newMovementVector = ProjectOnContactPlane((cameraDirection * playerInput.y) + cameraRightDirection * playerInput.x) ;

        newMovementVector = newMovementVector.normalized * playerInput.magnitude;
        desiredVelocity = newMovementVector * maxSpeed;

        float acceleration = grounded ? maxAcceleration : maxAirAcceleration;
        if (grounded && onSlope)
            acceleration = maxSlopeAcceleration;
        float maxSpeedChange = acceleration * Time.deltaTime;

        if (wallJumped && antiAirTimer <= 0 && playerInput != Vector2.zero)
            wallJumped = false;

        if (antiAirTimer <= 0 && !(wallJumped && playerInput == Vector2.zero))
        {
            //The problem lies here, where the maxspeedchange is a maximum + value, and not a percentage

            /*if (desiredVelocity.x != 0)
                velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, (float)Math.Sqrt(Math.Pow(desiredVelocity.x, 2)) / 100 * maxSpeedChange);
            else
                velocity.x = Mathf.MoveTowards(velocity.x, desiredVelocity.x, maxSpeedChange);
            if (desiredVelocity.z != 0)
                velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, (float)Math.Sqrt(Math.Pow(desiredVelocity.z, 2)) / 100 * maxSpeedChange);
            else
                velocity.z = Mathf.MoveTowards(velocity.z, desiredVelocity.z, maxSpeedChange);

            if (desiredVelocity.y != 0)
                velocity.y = Mathf.MoveTowards(velocity.y, desiredVelocity.y, (float)Math.Sqrt(Math.Pow(desiredVelocity.y, 2)) / 100 * maxSpeedChange);
            */

            if (grounded && !onSlope)
                velocity = Vector3.MoveTowards(velocity, desiredVelocity, maxSpeedChange);
            else
                velocity = Vector3.MoveTowards(velocity, new Vector3(desiredVelocity.x, velocity.y, desiredVelocity.z), maxSpeedChange);
        }
    }


    private void OnDrawGizmos()
    {
        if (rb == null)
            rb = GetComponentInChildren<Rigidbody>();

        if (rb != null)
        {

            Gizmos.color = Color.blue;

            Gizmos.DrawLine(rb.position, rb.position + velocity);

            Gizmos.color = Color.red;
            Vector3 gradient;

            gradient = ProjectOnContactPlane(Vector3.down);
            Gizmos.DrawLine(rb.position, rb.position + gradient.normalized * 3);
            Gizmos.DrawLine(rb.position, rb.position + Vector3.down * groundSnapProbeDistance);

            if (InputDistributor.playerInputActions != null)
            {
                Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
                playerInput = Vector2.ClampMagnitude(playerInput, 1f);

                if (playerInput != Vector2.zero)
                {
                    Vector3 cameraDirection = Camera.main.transform.forward;
                    Vector3 cameraRightDirection = Camera.main.transform.right;
                    cameraDirection = new Vector3(cameraDirection.x, 0, cameraDirection.z).normalized;
                    cameraRightDirection = new Vector3(cameraRightDirection.x, 0, cameraRightDirection.z).normalized;
                    Vector3 newMovementVector = ProjectOnContactPlane(cameraDirection) * playerInput.y;
                    newMovementVector += ProjectOnContactPlane(cameraRightDirection) * playerInput.x;


                    newMovementVector = newMovementVector.normalized * playerInput.magnitude;
                    desiredVelocity = newMovementVector * maxSpeed;

                    Gizmos.DrawLine(rb.position, rb.position + desiredVelocity.normalized * 3);
                    lastPlayerInput = playerInput;
                }
                else if (lastPlayerInput != null)
                {
                    Vector3 cameraDirection = Camera.main.transform.forward;
                    Vector3 cameraRightDirection = Camera.main.transform.right;
                    cameraDirection = new Vector3(cameraDirection.x, 0, cameraDirection.z).normalized;
                    cameraRightDirection = new Vector3(cameraRightDirection.x, 0, cameraRightDirection.z).normalized;
                    Vector3 newMovementVector = ProjectOnContactPlane(cameraDirection) * lastPlayerInput.y;
                    newMovementVector += ProjectOnContactPlane(cameraRightDirection) * lastPlayerInput.x;

                    newMovementVector = newMovementVector.normalized * lastPlayerInput.magnitude;
                    desiredVelocity = newMovementVector * maxSpeed;

                    Gizmos.DrawLine(rb.position, rb.position + desiredVelocity.normalized * 3);
                }
            }
            /*
                Gizmos.color = Color.red;
                Gizmos.DrawLine(rb.position, rb.position + -contactNormal.normalized * desiredHeight);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(rb.position + -contactNormal.normalized * desiredHeight, rb.position + -contactNormal.normalized * groundedDistance);
                Gizmos.color = Color.green;

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
                    Gizmos.DrawLine(startPos, startPos + Vector3.down * maxSlopeHeight);*/
        }
    }
    void Update()
    {

        if(Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = 0.1f;
        }

        velocity = rb.linearVelocity;

        CheckGrounded();
        if (grounded)
        {
            antiAirTimer = 0;
            wallJumped = false;
            wallRiding = false;
            inAir = false;

            if(!jumping)
                jumpPhase = 0;
        }

        else
        {
            contactNormal = Vector3.zero;

            if(!inAir)
            {
                jumpPhase = 1;
                inAir = true;
            }
        }

        AdjustVelocity();

        //CheckForWalls();
        AddGravity();
        AddSlope();
        if (grounded && !jumping)
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

    private void FixedUpdate()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;


        groundContactCount = 0;
        contactNormal = Vector3.zero;
    }

    void CheckGrounded()
    {
        if (groundContactCount > 0 || SnapToGround())
            grounded = true;
        else
            grounded = false;
    }
    bool SnapToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 2)
        {
            return false;
        }
        float speed = velocity.magnitude;
        if (speed > maxSnapSpeed)
        {
            return false;
        }
        if (!Physics.Raycast(rb.position, Vector3.down, out RaycastHit hit, groundSnapProbeDistance))
        {
            return false;
        }
        if (hit.normal.y < minGroundDotProduct)
        {
            return false;
        }
        groundContactCount = 1;
        contactNormal = hit.normal;
        //float dot = Vector3.Distance(rb.position, hit.point);

        //Debug.Log("Velocity: " + velocity + ", normal: " + hit.normal);
        //if (dot > 0f)   
        //    velocity = (velocity - hit.normal * dot).normalized * speed;
        //Debug.Log(velocity);

        rb.position = hit.point + Vector3.up * GetComponent<SphereCollider>().radius / 2;

        Debug.Log("Snapping to ground");
        return true;
    }

    void Jump()
    {
        if (grounded || jumpPhase <= maxAirJumps)
        {
            stepsSinceLastJump = 0;
            jumping = true;
            jumpTimer_countdown = jumpTimer;

            float jumpSpeed = jumpHeight;
            //float alignedSpeed = Vector3.Dot(velocity, contactNormal);

            //if (alignedSpeed > 0f)
            //{
            //    jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            //}

            if (!wallgrab)
            {
                velocity.y = 0;
                if (grounded)
                    velocity += contactNormal * jumpSpeed;
                else
                    velocity += Vector3.up * jumpSpeed;

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
        Debug.Log(contactNormal);
        Vector3 gradient;

        gradient = ProjectOnContactPlane(Vector3.down);
        rb.AddForce(gradient.normalized * slopeGlideStrength);
        Debug.Log(gradient.normalized);

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
