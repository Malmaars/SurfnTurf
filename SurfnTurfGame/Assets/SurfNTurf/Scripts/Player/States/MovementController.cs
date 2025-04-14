using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using UnityEngine.Events;
using Unity.Cinemachine;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;

//Version 2 of the movement controller will be using collissions instead of raycasts to check being grounded
public class MovementController : PlayerState, IMovement
{
	public bool gizmosOn;
	public CinemachineCamera playerCam;

	Rigidbody rb;
	public Rigidbody RB { get { return rb; } set { rb = value; } }

	public LayerMask waterlayers;

	[SerializeField]
	[ReadOnly]
	Vector3 velocity, desiredVelocity, extraVelocity, lastInputDirection3D, rememberedVelocity;

	public Vector3 Velocity { get { return velocity; } set { velocity = value; } }
	public Vector3 DesiredVelocity { get { return desiredVelocity; } set { desiredVelocity = value; } }
	public Vector3 LastInputDirection3D { get { return lastInputDirection3D; } set { lastInputDirection3D = value; } }
	public Vector2 LastPlayerInput { get { return lastPlayerInput; } set { lastPlayerInput = value; } }

	bool rememberVelocity;

	[SerializeField]
	[ReadOnly]
	bool limitVelocity;
	bool limitControl;

	public Transform playerVisual;
	public Transform PlayerVisual { get { return playerVisual; } set { playerVisual = value; } }
	
	public Animator animator;
	public Animator PlayerAnimator { get { return animator; } set { animator = value; } }


	[SerializeField]
	[ReadOnly]
	Vector2 lastPlayerInput;

	[SerializeField, Range(0f, 100f)]
	float visualRotationSpeed = 10f;

	Ability[] abilities;

	[Header("State settings")]
	public bool enableCookingStation;

	[Label("Ground Control")]
	public GroundControlValues gcv;
	public GroundControlValues GCV { get { return gcv; } set { gcv = value; }}

	[Label("Air Control")]
	public AirControlValues acv;
	public AirControlValues ACV { get { return acv; } set { acv = value; } }
	[Label("Jumping")]
	public JumpingValues jc;
	public JumpingValues JC { get { return jc; } set { jc = value; } }

	[Label("Wall Jumping")]
	public WallJumpingValues wjv;
	public WallJumpingValues WJV { get { return wjv; } set { wjv = value; } }
	[Label("Dashing")]
	public DashingVariables dv;
	public DashingVariables DV { get { return dv; } set { dv = value; } }
	[Label("Swiping")]
	public SwipingVariables swv;
	public SwipingVariables SWV{get{ return swv; }set { swv = value; } }
	[Label("Surfing")]
	public GroundSurfVariables suv;
	public GroundSurfVariables SUV { get { return suv; } set { suv = value; } }

	[Label("Advanced Movement")]
	public AdvancedMovement av;
	public AdvancedMovement AV { get { return av; } set { av = value; } }

	[Label("Interacting")]
	public InteractionVariables iv;
	public InteractionVariables IV { get { return iv; } set { iv = value; } }

	private void OnValidate()
	{
		gcv.minGroundDotProduct = Mathf.Cos(gcv.maxGroundAngle * Mathf.Deg2Rad);
		gcv.minSlopeDotProduct = Mathf.Cos(gcv.minSlopeAngle * Mathf.Deg2Rad);
	}

	private void Awake()
	{
		abilities = new Ability[] { 
			new Jump(this),
			new WallJump(this),
			new Spindash(this),
			new Dash(this),
			new AirMovement(this),
			new GroundMovement(this),
			new Leap(this),
			new Surf(this),
			new Swipe(this),
			new TwirlJump(this),
			new Dive(this),
			new TwirlSurf(this),
			new SurfParry(this),
			new Slide(this),
			new BoardTricks(this),
			new Interact(this)
		};

		foreach(Ability ability in abilities)
		{
			ability.RunOnAwake();
		}

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
		foreach (Ability ability in abilities) { ability.RunOnStart(); }
	}

	public override void InitStateTransitions()
	{
		base.InitStateTransitions();
		transitions.Add(new PlayerStateTransition(typeof(PauseState), () => nextState == typeof(PauseState)));
		if (enableCookingStation)
			transitions.Add(new PlayerStateTransition(typeof(CookingManager), () => nextState == typeof(CookingManager), new specialExit[] { ResetAnimator }));
		//transitions.Add(new PlayerStateTransition(typeof(InventoryMenuManager), () => nextState == typeof(InventoryMenuManager)));
		transitions.Add(new PlayerStateTransition(typeof(WaterMovementController), () => nextState == typeof(WaterMovementController), new specialExit[] { ResetAnimator }));
		transitions.Add(new PlayerStateTransition(typeof(TalkingState), () => nextState == typeof(TalkingState), new specialExit[] { StopVelocity }));
	}

	public override void EnterState()
	{
		ResetValues();

		if (rememberVelocity)
		{
			rememberVelocity = false;
			velocity = rememberedVelocity;
		}

		foreach (Ability ability in abilities) { ability.RunOnEnterState(); }

		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, OpenCookingStation);
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, OpenInventoryMenu);
		BlackBoard.cameraController.SwitchToCamera(playerCam, 0.5f);
		base.EnterState();
	}

	public override void ExitState()
	{
		foreach (Ability ability in abilities) { ability.RunOnExitState(); }

		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, OpenCookingStation);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, OpenInventoryMenu);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
		PlayerVFX.instance.runningDust.SendEvent("OnStop");
		animator.SetBool("Twirling", false);
		PlayerVFX.instance.twirl.gameObject.SetActive(false);
		base.ExitState();
	}

	public void SetNextState(System.Type _type)
	{
		nextState = _type;
	}

	public void StopVelocity()
	{
		Debug.Log("resetVelocity");
		velocity = Vector3.zero;
		rb.linearVelocity = velocity;
	}
	public void StopAndRememberVelocity()
	{
		rememberedVelocity = velocity;
		velocity = Vector3.zero;
		rememberVelocity = true;
	}


	void ResetValues()
	{
		acv.falling = false;
		jc.desiredJump = false;
		dv.dashing = false;
		dv.dashed = false;
		wjv.wallgrab = false;
		av.lv.leaping = false;
		av.lv.leapt = false;
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
		EvaluateCollision(collision);
	}

	void EvaluateCollision(Collision collision)
	{
		gcv.onSlope = false;

		gcv.allContactNormals = new Vector3[collision.contactCount];
		for (int i = 0; i < collision.contactCount; i++)
		{

			Vector3 normal = collision.GetContact(i).normal;
			gcv.allContactNormals[i] = normal;

			if (normal.y >= gcv.minGroundDotProduct)
			{
				if (normal.y <= gcv.minSlopeDotProduct)
					gcv.onSlope = true;

				gcv.groundContactCount++;
				gcv.contactNormal += normal;
				if (((1 << collision.GetContact(i).otherCollider.gameObject.layer) & waterlayers) != 0)
					nextState = typeof(WaterMovementController);

			}
		}
		if (gcv.groundContactCount > 1)
			gcv.contactNormal.Normalize();
		else if (gcv.groundContactCount == 0)
			gcv.contactNormal = Vector3.zero;
	}

	public Vector3 ProjectOnContactPlane(Vector3 vector)
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
			
			float inputMagnitude = gcv.SlowWalkingOn ? playerInput.magnitude : 1;
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

		float maxSpeed = gcv.maxSpeed;
		if (swv.swipingOnGround)
			maxSpeed = swv.moveSpeedWhileSwiping;
		if (suv.surfing)
			maxSpeed = suv.maxSurfSpeed;

		newMovementVector = newMovementVector.normalized * playerInput.magnitude;
		desiredVelocity = newMovementVector * maxSpeed;

		float acceleration = gcv.maxAcceleration;
		if (!gcv.grounded)
			acceleration = acv.maxAirAcceleration;
		if (gcv.grounded && gcv.onSlope)
			acceleration = gcv.maxSlopeAcceleration;

		float maxSpeedChange = acceleration * Time.deltaTime;

		if (wjv.wallJumped && acv.antiAirTimer <= 0)
			wjv.wallJumped = false;

		if (suv.surfing)
		{
			if (playerInput == Vector2.zero)
				desiredVelocity = lastInputDirection3D * maxSpeed;
		}

		else if (acv.antiAirTimer <= 0 && !(wjv.wallJumped && playerInput == Vector2.zero))
		{
			if (limitVelocity)
			{
				if (!limitControl)
				{
					if (gcv.grounded && !gcv.onSlope && !jc.jumping)
						velocity = Vector3.MoveTowards(velocity, desiredVelocity, maxSpeedChange);
					else
						velocity = Vector3.MoveTowards(velocity, new Vector3(desiredVelocity.x, velocity.y, desiredVelocity.z), maxSpeedChange);
				}
			}

			else
			{
				if (!limitControl)
				{
					if (gcv.grounded && !gcv.onSlope && !jc.jumping)
						velocity = Vector3.MoveTowards(velocity, desiredVelocity.normalized * velocity.magnitude, maxSpeedChange);
					else
						velocity = Vector3.MoveTowards(velocity, new Vector3(desiredVelocity.x, 0, desiredVelocity.z).normalized * new Vector3(velocity.x, 0, velocity.z).magnitude + new Vector3(0, velocity.y, 0), maxSpeedChange);
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
		if (!Application.IsPlaying(this))
			return;
		if (rb == null)
			rb = GetComponentInChildren<Rigidbody>();

		if (rb == null)
			return;

		foreach (Ability ability in abilities)
		{
			ability.RunOnDrawGizmos();
		}
	}
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			Time.timeScale = 0.1f;
		}
		if (Input.GetKeyDown(KeyCode.O))
		{
			Time.timeScale = 1f;
		}


		velocity = rb.linearVelocity;
		foreach (Ability ability in abilities)
		{
			ability.RunOnUpdateBeforeSetVelocity();
		}
		UpdateTimers();
		AdjustVelocity();

		foreach (Ability ability in abilities)
		{
			ability.RunOnUpdateDuringSetVelocity();
		}

		RotatePlayer();

		HandleLimiter();

		rb.linearVelocity = velocity;

		foreach (Ability ability in abilities)
		{
			ability.RunOnUpdateAfterSetVelocity();
		}

		UpdateAnimator();
	}

	private void FixedUpdate()
	{
		gcv.groundContactCount = 0;
		gcv.contactNormal = Vector3.zero;
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

	void RotatePlayer()
	{
		float rotationSpeed = wjv.wallJumped ? 20 : visualRotationSpeed;
		
		if(wjv.wallgrab)
		{
			Quaternion newRotation = Quaternion.LookRotation(new Vector3(-wjv.currentWallNormal.x, 0, -wjv.currentWallNormal.z));

			playerVisual.rotation = Quaternion.Slerp
			   (playerVisual.rotation, newRotation, visualRotationSpeed * Time.deltaTime);
			return;
		}
		
		if (new Vector3(velocity.x, 0, velocity.z).sqrMagnitude > 0.01f && new Vector3(velocity.x, 0, velocity.z) != Vector3.zero && playerVisual.forward != new Vector3(velocity.x, 0, velocity.z))
		{
			Quaternion newRotation = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));
			playerVisual.rotation = Quaternion.Slerp
			   (playerVisual.rotation, newRotation, visualRotationSpeed * Time.deltaTime);
		}
		else if (playerVisual.forward != lastInputDirection3D.normalized)
		{
			Quaternion newRotation = Quaternion.LookRotation(new Vector3(lastInputDirection3D.x, 0, lastInputDirection3D.z).normalized);
			playerVisual.rotation = Quaternion.Slerp
			   (playerVisual.rotation, newRotation, visualRotationSpeed * Time.deltaTime);

		}
	}

	void HandleLimiter()
	{
		if (dv.dashing || av.lv.leaping || wjv.wallJumpLimitVelocity || suv.surfing || av.div.diving)
			limitVelocity = false;
		else
			limitVelocity = true;

		if (dv.dashControlTimer > 0 || av.lv.leapControlTimer > 0 || wjv.wallJumpLimitVelocity || av.div.diving)
			limitControl = true;
		else
			limitControl = false;
	}



	public void OpenCookingStation(InputAction.CallbackContext context)
    {
		if (!gcv.grounded || jc.jumping || acv.falling || gcv.onSlope || jc.inAir || wjv.wallgrab || iv.interacting)
			return;

		velocity = Vector3.zero;
		animator.SetFloat("Speed", 0);
		rb.linearVelocity = Vector3.zero;
        nextState = typeof(CookingManager);
    }
    public void OpenInventoryMenu(InputAction.CallbackContext context)
	{
		if (iv.interacting)
			return;
		velocity = Vector3.zero;
		animator.SetFloat("Speed", 0);
		rb.linearVelocity = Vector3.zero;
		nextState = typeof(InventoryMenuManager);
	}

	void UpdateAnimator()
	{
		foreach (Ability ability in abilities)
		{
			ability.UpdateAnimator();
		}
    }

	void ResetAnimator()
	{
		animator.SetBool("WallSlide", false);
		animator.SetBool("Jumping", false);
		animator.SetBool("Falling", false);
		animator.SetBool("Dashing", false);
	}
}