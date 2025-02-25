using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using System.Collections.Generic;



//Version 2 of the movement controller will be using collissions instead of raycasts to check being grounded
public class MovementController : PlayerState
{
    Rigidbody rb;

    [SerializeField]
    [ReadOnly]
	Vector3 velocity, desiredVelocity, extraVelocity;

    public Transform playerVisual;
    public Animator animator;
	
    [SerializeField]
	[ReadOnly]
    Vector2 lastPlayerInput;

	[SerializeField, Range(0f, 100f)]
	float visualRotationSpeed = 10f;

    [Label("Ground Control")]
    public GroundControlValues gcv;
    [Label("Air Control")]
	public AirControlValues acv;
	[Label("Jumping")]
	public JumpingValues jc;

    public bool wallJumpingOn;
    [Label("Wall Jumping")]
	public WallJumpingValues wjv;


    private void OnValidate()
    {
		gcv.minGroundDotProduct = Mathf.Cos(gcv.maxGroundAngle * Mathf.Deg2Rad);
		gcv.minSlopeDotProduct = Mathf.Cos(gcv.minSlopeAngle * Mathf.Deg2Rad);
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
		gcv.onSlope = false;

        gcv.groundContacts.Clear();

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= gcv.minGroundDotProduct)
            {
                if (normal.y <= gcv.minSlopeDotProduct)
					gcv.onSlope = true;

				wjv.wallgrab = false;

                gcv.groundContacts.Add(collision.GetContact(i).otherCollider);
				gcv.contactNormal += normal;
            }
		}
        if (gcv.groundContacts.Count > 1)
			gcv.contactNormal.Normalize();
        else if (gcv.groundContacts.Count == 0)
			gcv.contactNormal = Vector3.zero;
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - gcv.contactNormal * Vector3.Dot(vector, gcv.contactNormal);
    }

    void AdjustVelocity()
    {
        Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();

        if (gcv.eightWayDirectionInput)
        {
			float inputMagnitude = playerInput.magnitude;
			playerInput = new Vector2(MathF.Round(playerInput.x), MathF.Round(playerInput.y));
			playerInput = playerInput.normalized * inputMagnitude;
		}
        else
        {
			playerInput = Vector2.ClampMagnitude(playerInput, 1f);
		}

		Vector3 cameraDirection = Camera.main.transform.forward;
        cameraDirection.y = 0;
        Vector3 cameraRightDirection = Camera.main.transform.right;
        cameraRightDirection.y = 0;
        Vector3 newMovementVector = ProjectOnContactPlane((cameraDirection * playerInput.y) + cameraRightDirection * playerInput.x) ;

        newMovementVector = newMovementVector.normalized * playerInput.magnitude;
        desiredVelocity = newMovementVector * gcv.maxSpeed;

        float acceleration = gcv.grounded ? gcv.maxAcceleration : acv.maxAirAcceleration;
        if (gcv.grounded && gcv.onSlope)
            acceleration = gcv.maxSlopeAcceleration;

            float maxSpeedChange = acceleration * Time.deltaTime;

        if (wjv.wallJumped && acv.antiAirTimer <= 0)
			wjv.wallJumped = false;

        if (acv.antiAirTimer <= 0 && !(wjv.wallJumped && playerInput == Vector2.zero))
        {
            if (gcv.grounded && !gcv.onSlope && !jc.jumping)
                velocity = Vector3.MoveTowards(velocity, desiredVelocity, maxSpeedChange);
            else
                velocity = Vector3.MoveTowards(velocity, new Vector3(desiredVelocity.x, velocity.y, desiredVelocity.z), maxSpeedChange);
        }
    }

 //   void RemoveColliderFromGroundContacts()
 //   {
	//	if (gcv.groundContacts.Count > 0)
	//	{
	//		foreach (Collider col in gcv.groundContacts)
	//		{
	//			if (col.GetComponent<FakeRigidbody>() == null)
	//				continue;
 //               extraVelocity -= col.GetComponent<FakeRigidbody>().velocity;
	//		}
	//	}
 //       extraVelocity = Vector3.zero;
	//}

 //   void MovingGroundCheck()
 //   {
 //       if (gcv.groundContacts.Count > 0)
 //       {
 //           foreach (Collider col in gcv.groundContacts)
 //           {
 //               if (col.GetComponent<FakeRigidbody>() == null)
 //                   continue;

 //               extraVelocity += col.GetComponent<FakeRigidbody>().velocity;
 //           }
 //       } 
 //   }

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
            Gizmos.DrawLine(rb.position, rb.position + Vector3.down * gcv.groundSnapProbeDistance);

			float angleStep = 360f;
			for (float i = 0; i < wjv.wallRaycastAmount; i++)
			{
				// Calculate the angle for the current raycast
				float angle = 90 + i * (angleStep / wjv.wallRaycastAmount);

				// Convert the angle to radians, then create a direction vector using cosine and sine for the x and z axes
				Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle));
				RaycastHit hit;

				Physics.Raycast(rb.position, direction, out hit, wjv.distanceUntilWallGrab);
				if (hit.collider != null && hit.normal.y >= 0f - wjv.maxWallAngleOffsetZeroToOne && hit.normal.y <= 0f + wjv.maxWallAngleOffsetZeroToOne)
				{
					//we're up against a wall

					if (Vector3.Dot(new Vector3(velocity.x, 0, velocity.z).normalized, direction) >= 1 - wjv.inputDirectionLeeway)
					{
						Gizmos.color = Color.green;
						Gizmos.DrawLine(rb.position, rb.position + direction * wjv.distanceUntilWallGrab);
					}

					else if (Vector3.Dot(new Vector3(velocity.x, 0, velocity.z).normalized, hit.point - rb.position) > 0)
					{
						Gizmos.color = Color.blue;
						Gizmos.DrawLine(rb.position, rb.position + direction * wjv.distanceUntilWallGrab);
					}
				}
				else
				{
					Gizmos.color = Color.red;
					Gizmos.DrawLine(rb.position, rb.position + direction * wjv.distanceUntilWallGrab);
				}
			}

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
                    desiredVelocity = newMovementVector * gcv.maxSpeed;

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
                    desiredVelocity = newMovementVector * gcv.maxSpeed;

                    Gizmos.DrawLine(rb.position, rb.position + desiredVelocity.normalized * 3);
                }
            }
        }
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = 0.1f;
        }

		velocity = rb.linearVelocity;
		UpdateTimers();
        CheckGrounded();
        UpdateGroundedValues();
        CheckFalling();
        if (wallJumpingOn)
            CheckForWalls();
        AdjustVelocity();
        AddGravity();
        AddSlope();
        HandleJumping();
        CheckLanding();
        RotatePlayer();
        rb.linearVelocity = velocity;
        UpdateAnimator();
	}

	private void FixedUpdate()
    {
		gcv.groundContacts.Clear();
		gcv.contactNormal = Vector3.zero;
    }

    void UpdateGroundedValues()
    {
		if (gcv.grounded)
		{
			acv.antiAirTimer = 0;
			wjv.wallJumped = false;
			wjv.wallRiding = false;
			jc.inAir = false;
			jc.coyoteTimeAvailable = true;

			if (!jc.jumping)
				jc.jumpPhase = 0;
		}

		else
		{
			gcv.contactNormal = Vector3.zero;

			if (!jc.inAir)
			{
				jc.jumpPhase = 1;
				jc.inAir = true;
			}
		}
	}
    void UpdateTimers()
    {
		if (jc.jumpBufferTimer > 0)
			jc.jumpBufferTimer -= Time.deltaTime;
		if (jc.coyoteTimer > 0)
			jc.coyoteTimer -= Time.deltaTime;
		if (acv.antiAirTimer > 0)
			acv.antiAirTimer -= Time.deltaTime;
	}

    void CheckFalling()
    {
        if (!gcv.grounded && jc.inAir && !jc.jumping && !Physics.Raycast(rb.position, Vector3.down, gcv.groundSnapProbeDistance))
        {
			acv.falling = true;
            if (jc.coyoteTimeAvailable)
            {
				jc.coyoteTimer = jc.coyoteTime;
				jc.coyoteTimeAvailable = false;
            }
        }
	}
	void CheckGrounded()
    {
        if (gcv.groundContacts.Count > 0)
			gcv.grounded = true;
        else
			gcv.grounded = false;
    }

	void CheckForWalls()
	{
		//send out a couple raycasts in multiple directions
		float angleStep = 360f;
		for (float i = 0; i < wjv.wallRaycastAmount; i++)
		{
			// Calculate the angle for the current raycast
			float angle = 90 + i * (angleStep / wjv.wallRaycastAmount);

			// Convert the angle to radians, then create a direction vector using cosine and sine for the x and z axes
			Vector3 direction = new Vector3(Mathf.Cos(Mathf.Deg2Rad * angle), 0, Mathf.Sin(Mathf.Deg2Rad * angle));
			RaycastHit hit;

			wjv.wallgrab = false; 
			Physics.Raycast(rb.position, direction, out hit, wjv.distanceUntilWallGrab);
			if (hit.collider != null && hit.normal.y >= 0f - wjv.maxWallAngleOffsetZeroToOne && hit.normal.y <= 0f + wjv.maxWallAngleOffsetZeroToOne)
			{
                if (Vector3.Dot(new Vector3(velocity.x, 0, velocity.z).normalized, hit.point - rb.position) >= 0)
					acv.antiAirTimer = 0;
                //we're up against a wall
                if (Vector3.Dot(new Vector3(velocity.x, 0, velocity.z).normalized, direction) >= 1 - wjv.inputDirectionLeeway)
                {
                    if (velocity.y < 0f)
                    {
                        velocity.x = 0f;
                        velocity.z = 0f;
						//player is aiming at the wall
						wjv.wallgrab = true;
						jc.jumpDirection = (hit.normal + Vector3.up) / 2;
                        break;
                    }
                }

                else if(Vector3.Dot(new Vector3(velocity.x, 0, velocity.z).normalized, hit.point - rb.position) >= 0)
                {
                    //we are touching a wall just not hugging it
                    if (Vector3.Dot(new Vector3(velocity.x, 0, velocity.z).normalized, hit.point - rb.position) > wjv.wallRidingMinimumOffset)
						jc.jumpDirection = ((hit.normal * 1.2f + Vector3.up + new Vector3(velocity.x, 0, velocity.z).normalized * 2f).normalized / 3);
                    else
                    {
						jc.jumpDirection = (hit.normal + Vector3.up) / 2;
                    }
					wjv.wallRiding = true;
					wjv.wallgrab = false;
                }
			}
           
		}

		//if the player is leaning against a wall, make slow them down;


	}

	void HandleJumping()
    {
		if (gcv.grounded && !jc.jumping)
			jc.jumpPhase = 0;

        if (!gcv.grounded && jc.jumping)
			jc.coyoteTimeAvailable = false;

		if (jc.desiredJump || (gcv.grounded && jc.jumpBufferTimer > 0))
		{
			jc.desiredJump = false;
			Jump();
		}

		if (jc.jumping && velocity.y < 0f)
			jc.jumping = false;
	}
	void Jump()
    {
        if (gcv.grounded || jc.jumpPhase <= jc.maxAirJumps || jc.coyoteTimer > 0)
        {
			jc.jumpBufferTimer = 0;
			jc.jumping = true;

            float jumpSpeed = jc.jumpHeight;
            //float alignedSpeed = Vector3.Dot(velocity, contactNormal);

            //if (alignedSpeed > 0f)
            //{
            //    jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
            //}

            if (!wjv.wallgrab)
            {
                velocity.y = 0;
                if (!gcv.onSlope)
                    velocity += Vector3.up * jumpSpeed;
                else
                    velocity += gcv.contactNormal * jumpSpeed;

                if (jumpSpeed > 0f)
					jc.jumpPhase++;
            }
            if (jc.coyoteTimer > 0)
				jc.jumpPhase = 1;

			jc.coyoteTimeAvailable = false;
			jc.coyoteTimer = 0;
			wjv.wallJumped = false;
        }
        else if(wjv.wallgrab || wjv.wallRiding)
        {
			jc.jumping = true;

            Vector3 newDir = (jc.jumpDirection + Vector3.up);
            newDir.Normalize();
            newDir = new Vector3(newDir.x, Mathf.Tan(Mathf.Deg2Rad * wjv.walljumpAngle), newDir.z);

            velocity = Vector3.zero;
            velocity += newDir * wjv.wallJumpForce;
			acv.antiAirTimer = wjv.wallJumpAntiAirTimer;
			wjv.wallJumped = true;
			wjv.wallgrab = false;
			wjv.wallRiding = false;
			jc.jumpPhase = 1;
        }
    }

    void RotatePlayer()
    {
        float rotationSpeed = wjv.wallJumped ? 20 : visualRotationSpeed;
		if (new Vector3(velocity.x, 0, velocity.z).sqrMagnitude > 0.01f && new Vector3(velocity.x, 0, velocity.z) != Vector3.zero && playerVisual.forward != new Vector3(velocity.x, 0, velocity.z))
		{
			Quaternion newRotation = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));

			playerVisual.localRotation = Quaternion.Slerp
			   (playerVisual.localRotation, newRotation, visualRotationSpeed * Time.deltaTime);
		}
	}

	void AddGravity()
    {
        if (gcv.grounded == true)
            return;

        if (rb.linearVelocity.y > acv.maximumDownVelocity && !gcv.onSlope)
        {
            //apply a consistent downforce, perhaps greater than normal gravity
            if (!wjv.wallgrab)
            {
                rb.AddForce(Vector3.up * acv.customGravityStrength * Time.deltaTime * 100);
            }
            else
                velocity.y = wjv.wallGrabGravity;
        }
    }

    void CheckLanding()
    {

        if (gcv.grounded && !jc.jumping && !gcv.onSlope && !jc.hasLanded && acv.falling)
        {
			jc.hasLanded = true;
			jc.hasLandedAnimation = true;
			acv.falling = false;
        }
        if (!gcv.grounded)
			jc.hasLanded = false;
    }

    void AddSlope()
    {
        if (!gcv.onSlope)
            return;
        Vector3 gradient;

        gradient = ProjectOnContactPlane(Vector3.down);
        rb.AddForce(gradient.normalized * gcv.slopeGlideStrength);
    }

    public void StartJump(InputAction.CallbackContext context)
    {
		jc.desiredJump = true;
		jc.jumpBufferTimer = jc.jumpBufferTime;
    }

    public void EndJump(InputAction.CallbackContext context)
    {
		jc.desiredJump = false;
    }

    void UpdateAnimator()
    {
        animator.SetFloat("Speed", new Vector2(velocity.x, velocity.z).magnitude / 10);

        if (jc.jumping != animator.GetBool("Jumping"))
            animator.SetBool("Jumping", jc.jumping);

        if (((!gcv.grounded && !jc.jumping && acv.falling) || gcv.onSlope) != animator.GetBool("Falling"))
            animator.SetBool("Falling", ((!gcv.grounded && !jc.jumping && acv.falling) || gcv.onSlope));

		if (gcv.grounded == true)
        {
            if (jc.hasLandedAnimation == true)
            {
                animator.SetTrigger("Landing");
				jc.hasLandedAnimation = false;
            }
        }
        else
			jc.hasLandedAnimation = false;
    }
}

[System.Serializable]
public class GroundControlValues
{

	public bool eightWayDirectionInput;

	[SerializeField, Range(0f, 100f)]
	public float maxSpeed = 10f;

	[SerializeField, Range(0f, 500f)]
	public float maxAcceleration = 10f;

	[SerializeField, Range(0f, 90f)]
	public float maxGroundAngle = 25f;
	[ReadOnly]
	[AllowNesting]
	public float minGroundDotProduct;

	[SerializeField, Range(0f, 90f)]
	public float minSlopeAngle = 25f;
	[ReadOnly]
	[AllowNesting]
	public float minSlopeDotProduct;

	public LayerMask groundedLayerMask;

    [ReadOnly]
    [AllowNesting]
    public List<Collider> groundContacts = new List<Collider>();

	[ReadOnly]
	[AllowNesting]
	public bool grounded, onSlope;

	[SerializeField, Range(0f, 100f)]
	public float maxSnapSpeed = 100f;

	[SerializeField, Min(0f)]
	public float groundSnapProbeDistance = 1f;

	[ReadOnly]
	[AllowNesting]
	public Vector3 contactNormal;

	[SerializeField, Range(0f, 100f)]
	public float slopeGlideStrength;

	[SerializeField, Range(0f, 100f)]
	public float maxSlopeAcceleration = 1f;

	[SerializeField, Range(0f, 10f)]
	public float forwardRaysDistance = 2f;
}

[System.Serializable]
public class AirControlValues
{
	[SerializeField, Range(0f, 500f)]
	public float maxAirAcceleration = 10f;

	[SerializeField, Range(-100f, 0f)]
	public float customGravityStrength;

	[SerializeField, Range(-200f, 0f)]
	public float maximumDownVelocity;

	[ReadOnly]
	[AllowNesting]
	public float antiAirTimer;

	[ReadOnly]
	[AllowNesting]
	public bool falling;

}

[System.Serializable]
public class JumpingValues
{
	[SerializeField, Range(0f, 100f)]
	public float jumpHeight = 2f;

	[SerializeField, Range(0, 5)]
	public int maxAirJumps = 0;

	[ReadOnly]
	[AllowNesting]
	public int jumpPhase;

	[ReadOnly]
	[AllowNesting]
	public Vector3 jumpDirection;

	[ReadOnly]
	[AllowNesting]
	public bool desiredJump, jumping, hasLanded, hasLandedAnimation, inAir;

	[SerializeField, Range(0, 5)]
	public float jumpBufferTime;
	
    [ReadOnly]
	[AllowNesting]
	public float jumpBufferTimer;
	
    [ReadOnly]
	[AllowNesting]
	public bool jumpBufferActive;

	[SerializeField, Range(0, 2)]
	public float coyoteTime;

	[ReadOnly]
	[AllowNesting]
	public float coyoteTimer;

	[ReadOnly]
	[AllowNesting]
	public bool coyoteTimeAvailable;
}
[System.Serializable]
public class WallJumpingValues
{

	[SerializeField, Range(4, 64)]
	public int wallRaycastAmount = 4;

	[SerializeField, Range(0, 4)]
	public float distanceUntilWallGrab = 1;

	[SerializeField, Range(0, 1)]
	public float maxWallAngleOffsetZeroToOne, inputDirectionLeeway = 0.2f, wallRidingMinimumOffset;

	[SerializeField, Range(-30, 0)]
	public float wallGrabGravity = -3f;

	[SerializeField, Range(0f, 100f)]
	public float wallJumpForce = 2f, wallRidingJumpForce = 2f;

	[SerializeField, Range(0f, 90f)]
	public float walljumpAngle = 45f;

	[SerializeField, Range(0f, 10f)]
	public float wallJumpAntiAirTimer = 0.5f;

	[ReadOnly]
	[AllowNesting]
	public bool wallgrab, wallRiding, wallJumped;
}

