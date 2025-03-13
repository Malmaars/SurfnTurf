using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using UnityEngine.Events;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEditor.Rendering.LookDev;



//Version 2 of the movement controller will be using collissions instead of raycasts to check being grounded
public class MovementController : PlayerState
{
	public bool gizmosOn;
	public CinemachineCamera playerCam;

    Rigidbody rb;

	public LayerMask waterlayers;

    [SerializeField]
    [ReadOnly]
	Vector3 velocity, desiredVelocity, extraVelocity, lastInputDirection3D;

	[SerializeField]
	[ReadOnly]
	bool limitVelocity;
	bool limitControl;

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

	[Label("Dashing")]
	public DashingVariables dv;
	
	[Label("Leaping")]
	public LeapingVariables lv;

	[Label("Interacting")]
    public InteractionVariables iv;
	


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
		jc.jumpDirection = Vector3.zero;
    }
    private void Start()
    {
        rb = GetComponentInChildren<Rigidbody>();
	}

	public override void EnterState()
	{
		ResetValues();
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, OpenCookingStation);
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, OpenInventoryMenu);
		InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Jump, EndJump);
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Dash, StartDash);
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Interactions.Talk, Interact);
		BlackBoard.cameraController.SwitchToCamera(playerCam);
		base.EnterState();
	}

	public override void ExitState()
	{
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, OpenCookingStation);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, OpenInventoryMenu);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Jump, EndJump);
        InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Interactions.Talk, Interact);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Dash, StartDash);
		base.ExitState();
	}

	void ResetValues()
	{
		acv.falling = false;
		jc.desiredJump = false;
	}
	void OnCollisionEnter(Collision collision)
    {
		if (!this.enabled)
			return;
        //onGround = true;
        EvaluateCollision(collision);
    }

    void OnCollisionStay(Collision collision)
	{
		if (!this.enabled)
			return;
		//onGround = true;
		EvaluateCollision(collision);
    }
    private void OnCollisionExit(Collision collision)
	{
		if (!this.enabled)
			return;
		//Debug.Log("triggering on collisionexit");
		EvaluateCollision(collision);
    }

    void EvaluateCollision(Collision collision)
    {
		gcv.onSlope = false;

        for (int i = 0; i < collision.contactCount; i++)
        {
            Vector3 normal = collision.GetContact(i).normal;
            if (normal.y >= gcv.minGroundDotProduct)
            {
                if (normal.y <= gcv.minSlopeDotProduct)
					gcv.onSlope = true;

                gcv.groundContactCount++;
				gcv.contactNormal += normal;
				if (((1 << collision.GetContact(i).otherCollider.gameObject.layer) & waterlayers) != 0)
					BlackBoard.playerManager.SwitchState(typeof(WaterMovementController));

            }
		}
        if (gcv.groundContactCount> 1)
			gcv.contactNormal.Normalize();
        else if (gcv.groundContactCount == 0)
			gcv.contactNormal = Vector3.zero;
    }

    Vector3 ProjectOnContactPlane(Vector3 vector)
    {
        return vector - gcv.contactNormal * Vector3.Dot(vector, gcv.contactNormal);
    }

    void AdjustVelocity()
    {
        Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
		if (iv.interacting)
			playerInput = Vector2.zero;

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
        Vector3 newMovementVector = ProjectOnContactPlane((cameraDirection * playerInput.y) + cameraRightDirection * playerInput.x);

		if (playerInput != Vector2.zero)
			lastInputDirection3D = newMovementVector.normalized;

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
			if (limitVelocity)
			{
				if (limitControl)
				{
					if (gcv.grounded && !gcv.onSlope && !jc.jumping)
						velocity = Vector3.MoveTowards(velocity, desiredVelocity, maxSpeedChange);
					else
						velocity = Vector3.MoveTowards(velocity, new Vector3(desiredVelocity.x, velocity.y, desiredVelocity.z), maxSpeedChange);
				}
			}

			else
			{
				if (limitControl)
				{
					if (gcv.grounded && !gcv.onSlope && !jc.jumping)
						velocity = Vector3.MoveTowards(velocity, desiredVelocity.normalized * velocity.magnitude, maxSpeedChange);
					else
						velocity = Vector3.MoveTowards(velocity, new Vector3(desiredVelocity.x, 0, desiredVelocity.z).normalized * new Vector3(velocity.x, 0, velocity.z).magnitude + new Vector3(0,velocity.y,0), maxSpeedChange);
				}
				if (gcv.onSlope)
					dv.dashLengthTimer = 0;
			}
        }
    }

    private void OnDrawGizmos()
    {
		if (!gizmosOn)
			return;

        if (rb == null)
            rb = GetComponentInChildren<Rigidbody>();

        if (rb != null)
        {
			if (gcv.gizmosOn)
			{
				Gizmos.color = Color.blue;

				Gizmos.DrawLine(rb.position, rb.position + velocity);

				Gizmos.color = Color.red;
				Vector3 gradient;

				gradient = ProjectOnContactPlane(Vector3.down);
				Gizmos.DrawLine(rb.position, rb.position + gradient.normalized * 3);
				Gizmos.DrawLine(rb.position, rb.position + Vector3.down * gcv.groundSnapProbeDistance);


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

			if (wjv.gizmosOn)
			{
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
			}

			if (iv.gizmosOn)
			{
				Gizmos.color = Color.blue;

				Gizmos.DrawWireSphere(rb.position, iv.measuringDistance);
                Gizmos.color = new Color32(255,255,255,50);
                Gizmos.DrawSphere(rb.position, iv.measuringDistance);
			}

        }
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.P))
        {
            Time.timeScale = 0.1f;
        }
		if(Input.GetKeyDown(KeyCode.O))
        {
            Time.timeScale = 1f;
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
		HandleLeap();
		HandleDash();

		HandleLimiter();
		
        rb.linearVelocity = velocity;
		CheckForInteractibles();
        UpdateAnimator();
	}

	private void FixedUpdate()
    {
		gcv.groundContactCount = 0;
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

			if (!jc.jumping)
			{
				jc.jumpPhase = 0;
				jc.coyoteTimeAvailable = true;
			}
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
        if (gcv.groundContactCount > 0)
			gcv.grounded = true;
        else
			gcv.grounded = false;
    }

	void CheckForWalls()
	{
		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();

		bool wallgrabbed = (wjv.wallgrab || wjv.wallRiding);

		if (wjv.wallJumpCoyoteTimer > 0)
			wjv.wallJumpCoyoteTimer -= Time.deltaTime;

		if (wjv.wallJumpLimitVelocity && gcv.grounded || (acv.antiAirTimer <= 0 && playerInput != Vector2.zero))
			wjv.wallJumpLimitVelocity = false;

		if (wjv.wallJumpCooldownTimer > 0 || acv.antiAirTimer > 0 || Physics.Raycast(rb.position, Vector3.down, wjv.minimumDistanceFromGround))
		{
			wjv.wallgrab = false;
			wjv.wallRiding = false;

			if (wjv.wallJumpCooldownTimer > 0)
				wjv.wallJumpCooldownTimer -= Time.deltaTime;
			return;
		}

		bool noWalls = true;


		//send out a couple raycasts in multiple directions
		float angleStep = 360f;

		List<Vector3> wallAngles = new List<Vector3>();

		wjv.wallgrab = false;
		bool debugCheck = false;

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
                if (playerInput != Vector2.zero && Vector3.Dot(new Vector3(lastInputDirection3D.x, 0, lastInputDirection3D.z).normalized, hit.point - rb.position) >= 0)
					acv.antiAirTimer = 0;
				//we're up against a wall

                if (playerInput != Vector2.zero && Vector3.Dot(new Vector3(lastInputDirection3D.x, 0, lastInputDirection3D.z).normalized, -hit.normal) >= 1 - wjv.inputDirectionLeeway)
                {
					if (velocity.y <= 0f)
                    {
                        velocity.x = 0f;
                        velocity.z = 0f;
						//player is aiming at the wall
						wjv.wallgrab = true;
						wjv.jumpDirection = (hit.normal + Vector3.up) / 2;
						noWalls = false;
						wallAngles.Clear();
                        break;
                    }
                }

                else if(Vector3.Dot(new Vector3(velocity.x, 0, velocity.z).normalized, hit.point - rb.position) >= 0)
                {
					Vector3 newAngle;
					//we are touching a wall just not hugging it
					newAngle = hit.normal;

					if (!wallAngles.Contains(newAngle))
						wallAngles.Add(newAngle);

					noWalls = false;
					wjv.wallRiding = true;
					wjv.wallgrab = false;
                }
			}
		}

		if (wallAngles.Count > 0)
		{
			Vector3 newDirection = Vector3.zero;

			foreach (Vector3 v3 in wallAngles)
			{
				newDirection += v3;
			}

			newDirection.Normalize();

			if (velocity != Vector3.zero && Vector3.Dot(new Vector3(velocity.x, 0, velocity.z).normalized, newDirection.normalized) > wjv.wallRidingMinimumOffset)
			{
				newDirection = ((newDirection.normalized * 1.2f + Vector3.up + new Vector3(velocity.x, 0, velocity.z).normalized * 2f) / 3);
			}
			else
				newDirection = (newDirection.normalized + Vector3.up) / 2;


			if (newDirection != Vector3.up)
				wjv.jumpDirection = newDirection;
			else
				noWalls = true;
		}

		if(noWalls)
		{
			wjv.wallgrab = false;
			wjv.wallRiding = false;
		}

		if(wallgrabbed && (!wjv.wallgrab && !wjv.wallRiding))
		{
			//start the coyote timer
			wjv.wallJumpCoyoteTimer = wjv.wallJumpCoyoteTime;
			wallgrabbed = false;
		}
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
		{
			jc.jumping = false;
		}
	}
	void Jump()
    {
		int newMaxAirJumps = jc.maxAirJumps;
		newMaxAirJumps = (dv.dashingGivesExtraJump && dv.dashed&& dv.dashCoyoteTimer > 0) ? newMaxAirJumps + 1 : newMaxAirJumps;

		if (!lv.leapt && !gcv.onSlope && ((dv.dashing || lv.leapCoyoteTimer > 0) && (gcv.grounded || Physics.Raycast(rb.position, Vector3.down, lv.maxDistanceFromGround)) || (dv.dashed && (gcv.grounded || Physics.Raycast(rb.position, Vector3.down, lv.maxDistanceFromGround)) && jc.jumpBufferTimer > 0)))
		{
			Leap();
		}
		else if (wjv.wallgrab || wjv.wallRiding || wjv.wallJumpCoyoteTimer > 0)
		{
			jc.jumping = true;

			Vector3 newDir = (wjv.jumpDirection + Vector3.up);
			newDir.Normalize();
			newDir = new Vector3(newDir.x, Mathf.Tan(Mathf.Deg2Rad * wjv.walljumpAngle), newDir.z);

			velocity = Vector3.zero;
			velocity += newDir * wjv.wallJumpForce;
			acv.antiAirTimer = wjv.wallJumpAntiAirTimer;
			wjv.wallJumpCooldownTimer = wjv.wallJumpCooldown;
			wjv.wallJumpCoyoteTimer = 0;
			wjv.wallJumped = true;
			wjv.wallgrab = false;
			wjv.wallRiding = false;
			wjv.wallJumpLimitVelocity = true;
			jc.jumpPhase = 1;
		}
		else 
		{
			if (dv.breakDashWithJump && !dv.airJumped && jc.inAir && (dv.dashing || dv.dashCoyoteTimer > 0))
			{
				dv.airJumped = true;
				velocity = Vector3.zero;
				dv.dashing = false;
			}

			if (!lv.leaping && (gcv.grounded || jc.jumpPhase <= newMaxAirJumps || jc.coyoteTimer > 0))
			{
				Debug.Log("Normal Jump");
				jc.jumpBufferTimer = 0;
				jc.jumping = true;

				float jumpSpeed = jc.jumpHeight;

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
				dv.dashing = false;
				lv.leaping = false;
				jc.onJump.Invoke();
			}
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
		else if (playerVisual.forward != lastInputDirection3D.normalized)
		{
			Quaternion newRotation = Quaternion.LookRotation(lastInputDirection3D.normalized);

			playerVisual.localRotation = Quaternion.Slerp
			   (playerVisual.localRotation, newRotation, visualRotationSpeed * Time.deltaTime);

		}
	}

	void AddGravity()
    {
        if (gcv.grounded == true || dv.gravityOff)
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

	void CheckForInteractibles()
	{
		//do a physics sphere check around the player, and check if anything is interactible within that
		if (iv.interacting)
		{
			if (iv.currentInteractible != null)
				iv.currentInteractible.RemoveHighlight();
			return;
		}
		Collider[] collidersClose = Physics.OverlapSphere(rb.position, iv.measuringDistance);

		Interactible previousInteractable = iv.currentInteractible;
		Interactible closestInteractible = null;

		foreach (Collider collider in collidersClose)
		{
			if (collider.GetComponent<Interactible>() == null)
				continue;

			if(closestInteractible == null || Vector3.Distance(collider.transform.position, rb.transform.position) < Vector3.Distance(closestInteractible.transform.position, rb.transform.position))
			{
				closestInteractible = collider.GetComponent<Interactible>();
			}
		}

		iv.currentInteractible = closestInteractible;

		if (previousInteractable != null && previousInteractable != iv.currentInteractible)
			previousInteractable.RemoveHighlight();
        
        if (iv.currentInteractible != null)
			iv.currentInteractible.Highlight();
	}

	void Interact(InputAction.CallbackContext context)
	{
		if (iv.currentInteractible == null)
			return;
		iv.interacting = iv.currentInteractible.InteractWith();
	}

	void HandleLimiter()
	{
		if (dv.dashing || lv.leaping || wjv.wallJumpLimitVelocity)
			limitVelocity = false;
		else
			limitVelocity = true;

		if (dv.dashControlTimer > 0 || lv.leapControlTimer > 0 || wjv.wallJumpLimitVelocity)
			limitControl = false;
		else
			limitControl = true;
	}


	

	void HandleLeap()
	{

		if (lv.leapLengthTimer > 0)
		{
			lv.leapLengthTimer -= Time.deltaTime;
			if (lv.leapLengthTimer <= 0)
				lv.leaping = false;
		}

		if (lv.leapControlTimer > 0)
			lv.leapControlTimer -= Time.deltaTime;

		if (lv.leapCoyoteTimer > 0)
			lv.leapCoyoteTimer -= Time.deltaTime;

		if (gcv.grounded)
		{
			if (dv.dashingResetsLeap && dv.dashed && dv.dashTimer <= 0)
			{
				lv.leapCoyoteTimer = lv.leapCoyoteTime;
			}

			if (!lv.leaping)
			{
				lv.leapt = false;
			}
		}

	}

	void Leap()
	{
		//perform a leap if you're close enough to the ground
		RaycastHit hit;

		if (gcv.grounded || jc.coyoteTime > 0 || Physics.Raycast(rb.position, Vector3.down, out hit, lv.maxDistanceFromGround))
		{
			if (lv.leapingResetsVelocity)
				velocity = Vector3.zero;
			//leap in a forward direction instead of straight up

			Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();


			if (playerInput != Vector2.zero)
			{

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
				Vector3 newMovementVector = ProjectOnContactPlane((cameraDirection * playerInput.y) + cameraRightDirection * playerInput.x);

				velocity += new Vector3(newMovementVector.x * lv.forwardSpeed, lv.upwardSpeed, newMovementVector.z * lv.forwardSpeed);
			}
			else
				velocity += new Vector3(dv.LastHorizontalDirection.x * lv.forwardSpeed, lv.upwardSpeed, dv.LastHorizontalDirection.z * lv.forwardSpeed);

			lv.leapAnimation = true;
			lv.leaping = true;
			lv.leapt = true;
			lv.leapLengthTimer = lv.leapLength;
			lv.leapCoyoteTimer = 0;
			lv.leapControlTimer = lv.leapControlTime;

			if (lv.leapingResetsDash)
				dv.dashed = false;

			lv.onLeap.Invoke();
		}
	}

	void StartDash(InputAction.CallbackContext context)
	{
		dv.desiredDash = true;
	}
void HandleDash() 
	{

		if (velocity != Vector3.zero && !(velocity.x == 0 && velocity.z == 0))
			dv.LastHorizontalDirection = velocity.normalized;

		if (dv.dashing)
		{
			if (dv.dashLengthTimer > 0)
			{
				dv.dashLengthTimer -= Time.deltaTime;
			}
			else
			{
				dv.dashCoyoteTimer = dv.dashCoyoteTime;
				dv.dashing = false;
			}
			return;
		}

		else if(dv.dashTimer > 0)
		{
			dv.dashTimer -= Time.deltaTime;
		}

		if (!dv.dashing)
		{
			dv.gravityOff = false;
			if (dv.immediateStop)
				velocity = Vector3.zero;

			if (dv.dashed && dv.dashCoyoteTimer > 0)
				dv.dashCoyoteTimer -= Time.deltaTime;
		}

		if (dv.dashControlTimer > 0)
			dv.dashControlTimer -= Time.deltaTime;

		if (gcv.grounded)
		{ 
			if (dv.dashTimer <= 0)
				dv.dashed = false;
		}

		if (dv.desiredDash)
			Dash();

	}
	void Dash()
	{

		Vector3 desiredDirection;
		dv.desiredDash = false;
		if (dv.dashed)
			return;

		velocity = Vector3.zero;
		if (dv.threeDimensionalDash)
			desiredDirection = dv.LastHorizontalDirection;

		else
		{
			if (gcv.contactNormal == Vector3.zero || gcv.contactNormal.y < 0 || gcv.onSlope)
			{
				if(dv.fullDashControl)
					desiredDirection = new Vector3(lastInputDirection3D.x, 0, lastInputDirection3D.z).normalized;
				else
					desiredDirection = new Vector3(dv.LastHorizontalDirection.x, 0, dv.LastHorizontalDirection.z).normalized;
			}
			else
			{
				if (dv.fullDashControl)
					desiredDirection = ProjectOnContactPlane(new Vector3(lastInputDirection3D.x, 0, lastInputDirection3D.z).normalized).normalized;
				else
					desiredDirection = ProjectOnContactPlane(new Vector3(dv.LastHorizontalDirection.x, 0, dv.LastHorizontalDirection.z).normalized).normalized;
			}
		}
		velocity += desiredDirection * dv.dashSpeed;
		dv.dashed = true;
		dv.dashing = true;
		dv.airJumped = false;
		dv.dashTimer = dv.dashCooldown;
		dv.dashLengthTimer = dv.dashLength;
		dv.dashControlTimer = dv.dashControlTime;
		dv.startDash = true;
		dv.gravityOff = true;
		dv.onDash.Invoke();

		if (dv.dashingResetsLeap)
			lv.leapt = false;
	}

	public void StartJump(InputAction.CallbackContext context)
    {
		if (iv.interacting)
			return;

		jc.desiredJump = true;
		jc.jumpBufferTimer = jc.jumpBufferTime;
    }

	public void OpenCookingStation(InputAction.CallbackContext context)
    {
		if (!gcv.grounded || jc.jumping || acv.falling || gcv.onSlope || jc.inAir || wjv.wallgrab)
			return;

		velocity = Vector3.zero;
		animator.SetFloat("Speed", 0);
		rb.linearVelocity = Vector3.zero;
		BlackBoard.playerManager.SwitchState(typeof(CookingManager));
	}
	public void OpenInventoryMenu(InputAction.CallbackContext context)
	{
		velocity = Vector3.zero;
		animator.SetFloat("Speed", 0);
		rb.linearVelocity = Vector3.zero;
		BlackBoard.playerManager.SwitchState(typeof(InventoryMenuManager));
	}


	public void EndJump(InputAction.CallbackContext context)
    {
		if (iv.interacting)
			return;

		jc.desiredJump = false;
    }

	void UpdateAnimator()
	{
		animator.SetFloat("Speed", new Vector2(velocity.x, velocity.z).magnitude / 10);

		if (wjv.wallgrab && !wjv.wallgrabAnimation)
		{
			animator.SetBool("WallSlide", true);
			wjv.wallgrabAnimation = true;
		}
		else if (!wjv.wallgrab)
		{
			animator.SetBool("WallSlide", false);
			wjv.wallgrabAnimation = false;
		}
        
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

		if (dv.startDash)
		{
			dv.startDash = false;
			animator.SetTrigger("Dash");
		}
		animator.SetBool("Dashing", dv.dashing);

		if (lv.leapAnimation && lv.leaping)
		{
			lv.leapAnimation = false;
			animator.SetTrigger("Leap");
		}
    }
}

[System.Serializable]
public class GroundControlValues
{

    public bool gizmosOn;

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
	public int groundContactCount;

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

	public UnityEvent onJump;
}
[System.Serializable]
public class WallJumpingValues
{
    public bool gizmosOn;

	[ReadOnly]
	[AllowNesting]
	public Vector3 jumpDirection;

	[SerializeField, Range(4, 64)]
	public int wallRaycastAmount = 4;

	[SerializeField, Range(0, 4)]
	public float distanceUntilWallGrab = 1;
	[SerializeField, Range(0, 4)]
	public float minimumDistanceFromGround = 1;

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
	
	[SerializeField, Range(0f, 1f)]
	public float wallJumpCooldown = 0.5f;

	[SerializeField, Range(0f, 2f)]
	public float wallJumpCoyoteTime = 0.2f;

	[ReadOnly]
	[AllowNesting]
	public float wallJumpCoyoteTimer;

	[ReadOnly]
	[AllowNesting]
	public bool wallJumpLimitVelocity;

	[ReadOnly]
	[AllowNesting]
	public float wallJumpCooldownTimer;

	[ReadOnly]
	[AllowNesting]
	public bool wallgrab, wallRiding, wallJumped;

    [ReadOnly]
    [AllowNesting]
    public bool wallgrabAnimation;
}

[System.Serializable]
public class InteractionVariables
{
	public bool gizmosOn;

	[ReadOnly]
	[AllowNesting]
	public Interactible currentInteractible;

	[ReadOnly]
	[AllowNesting]
	public bool interacting;

	[SerializeField, Range(0f, 10f)]
	public float measuringDistance;
}


[System.Serializable]
public class DashingVariables
{
	public bool threeDimensionalDash;
	public bool immediateStop;

	public bool fullDashControl;
	public bool dashingResetsLeap;
	public bool dashingGivesExtraJump;
	public bool breakDashWithJump;

	[ReadOnly]
	[AllowNesting]
	public bool gravityOff;

	[SerializeField, Range(0f, 100f)]
	public float dashSpeed;

	[SerializeField, Range(0f, 10f)]
	public float dashCooldown;

	[ReadOnly]
	[AllowNesting]
	public float dashTimer;
	
	[ReadOnly]
	[AllowNesting]
	public Vector3 LastHorizontalDirection;

	[SerializeField, Range(0f, 10f)]
	public float dashLength;

	[ReadOnly]
	[AllowNesting]
	public float dashLengthTimer;

	[SerializeField, Range(0f, 2f)]
	public float dashControlTime;

	[ReadOnly]
	[AllowNesting]
	public float dashControlTimer;

	[SerializeField, Range(0f, 2f)]
	public float dashCoyoteTime = 0.2f;

	[ReadOnly]
	[AllowNesting]
	public float dashCoyoteTimer;

	[ReadOnly]
	[AllowNesting]
	public bool startDash;


	[ReadOnly]
	[AllowNesting]
	public bool desiredDash;

	[ReadOnly]
	[AllowNesting]
	public bool dashing;

	[ReadOnly]
	[AllowNesting]
	public bool dashed;	
	
	[ReadOnly]
	[AllowNesting]
	public bool airJumped;

	public UnityEvent onDash;
}

[System.Serializable]
public class LeapingVariables
{
	public bool leapingResetsVelocity;
	public bool leapingResetsDash;
	[SerializeField, Range(0f, 100f)]
	public float upwardSpeed;
	[SerializeField, Range(0f, 100f)]
	public float forwardSpeed;

	[SerializeField, Range(0f, 10f)]
	public float maxDistanceFromGround;

	[SerializeField, Range(0f, 10f)]
	public float leapLength;

	[ReadOnly]
	[AllowNesting]
	public float leapLengthTimer;

	[SerializeField, Range(0f, 2f)]
	public float leapControlTime;

	[ReadOnly]
	[AllowNesting]
	public float leapControlTimer;

	[SerializeField, Range(0f, 2f)]
	public float leapCoyoteTime;

	[ReadOnly]
	[AllowNesting]
	public float leapCoyoteTimer;

	[ReadOnly]
	[AllowNesting]
	public bool desiredLeap;
	[ReadOnly]
	[AllowNesting]
	public bool leapAvailable;

	[ReadOnly]
	[AllowNesting]
	public bool leapAnimation;

	[ReadOnly]
	[AllowNesting]
	public bool leapt;

	[ReadOnly]
	[AllowNesting]
	public bool leaping;

	public UnityEvent onLeap;
}