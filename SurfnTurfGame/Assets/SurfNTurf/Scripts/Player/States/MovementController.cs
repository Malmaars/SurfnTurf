using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using NaughtyAttributes;
using UnityEngine.Events;
using Unity.Cinemachine;
using Unity.VisualScripting;
using System.Runtime.CompilerServices;
using FMODUnity;
using FMOD.Studio;

//Version 2 of the movement controller will be using collissions instead of raycasts to check being grounded
public class MovementController : PlayerState
{
	public bool gizmosOn;
	public CinemachineCamera playerCam;
	
	[SerializeField]
	[ReadOnly]
	public Rigidbody rb;

	public LayerMask waterlayers;

	[SerializeField]
	[ReadOnly]
	public Vector3 velocity, desiredVelocity, extraVelocity, lastInputDirection3D, rememberedVelocity;

	bool rememberVelocity;

	[SerializeField]
	[ReadOnly]
	bool limitVelocity;
	
	[SerializeField]
	[ReadOnly]
	bool limitControl;

	[SerializeField]
	[ReadOnly]
	bool limitAllControl;

	public Transform playerVisual;
	
	public Animator animator;

	[SerializeField]
	[ReadOnly]
	public Vector2 lastPlayerInput;

	[SerializeField, Range(0f, 100f)]
	float visualRotationSpeed = 10f;

	Ability[] abilities;

	[Header("State settings")]
	public bool enableCookingStation;

	[Label("Ground Control")]
	public GroundControlValues gcv;

	[Label("Air Control")]
	public AirControlValues acv;

	[Label("Jumping")]
	public JumpingValues jc;

	[Label("Ledge Grab")]
	public LedgeGrabVariables lgv;


	[Label("Wall Jumping")]
	public WallJumpingValues wjv;
	[Label("Dashing")]
	public DashingVariables dv;
	[Label("Swiping")]
	public SwipingVariables swv;
	[Label("Surfing")]
	public GroundSurfVariables suv;

	[Label("Advanced Movement")]
	public AdvancedMovement av;

	[Label("Interacting")]
	public InteractionVariables iv;
	[Label("Canon")]
	public CanonVariables cv;
	[Label("LowGravityZone")]
	public LowGravityZoneVariables lgz;

	private void OnValidate()
	{
		if (abilities != null && abilities.Length > 0)
			foreach (Ability ability in abilities) { ability.RunOnValidate(); }
	}

	private void Awake()
	{
		abilities = new Ability[] {
			new Grind(this),
			new Jump(this),
			new WallJump(this),
			new Spindash(this),
			new Leap(this),
			new Dash(this),
			new AirMovement(this),
			new GroundMovement(this),
			new Surf(this),
			new Swipe(this),
			new TwirlJump(this),
			new Dive(this),
			new TwirlSurf(this),
			new SurfParry(this),
			new Slide(this),
			new BoardTricks(this),
			new LedgeGrab(this),
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

		transitions.Add(new PlayerStateTransition(typeof(WaterMovementController), () => nextState == typeof(WaterMovementController), new specialExit[] { ResetAnimator }));
		transitions.Add(new PlayerStateTransition(typeof(TalkingState), () => nextState == typeof(TalkingState), new specialExit[] { StopVelocity, ResetAnimator }));
		transitions.Add(new PlayerStateTransition(typeof(PickUpState), () => nextState == typeof(PickUpState), new specialExit[] { StopAndRememberVelocity }));
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
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, HandleCookingStation);
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Interactions.Grab, gameObject.GetComponent<Grab>().GrabObject);
		BlackBoard.cameraController.SwitchToCamera(playerCam, 0.5f);
		BlackBoard.playerVFX.landTrail.Reinit();
		base.EnterState();
	}

	public override void ExitState()
	{
		foreach (Ability ability in abilities) { ability.RunOnExitState(); }

		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenCookingStation, HandleCookingStation);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Interactions.Grab, gameObject.GetComponent<Grab>().GrabObject);
		gameObject.GetComponent<Grab>().ReleaseObject(0);
		PlayerVFX.instance.runningDust.SendEvent("OnStop");
		BlackBoard.playerVFX.landTrail.SetBool("On", false);
		animator.SetBool("Twirling", false);
		animator.SetBool("Running", false);
		PlayerVFX.instance.twirl.gameObject.SetActive(false);
		acv.fallingLoopInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		base.ExitState();
	}

	public void SetNextState(System.Type _type)
	{
		nextState = _type;
	}

	public void StopVelocity()
	{
		velocity = Vector3.zero;
		rb.linearVelocity = velocity;
	}
	public void StopAndRememberVelocity()
	{
		rememberedVelocity = velocity;
		velocity = Vector3.zero;
		rb.linearVelocity = velocity;
		rememberVelocity = true;
	}

	public void ResetValues()
	{
		foreach (Ability ability in abilities) { ability.ResetValues(); }
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
			playerInput = Vector2.ClampMagnitude(playerInput, 1f);

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

		maxSpeed = gcv.comboAddsSpeed ? (maxSpeed + (ComboMeter.currentCombo)) : maxSpeed;
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

			if (gcv.grounded && !gcv.onSlope && !jc.jumping)
				animator.SetFloat("RunSpeed",playerInput.magnitude);
			else
				animator.SetFloat("RunSpeed", 1);

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
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawSphere(rb.position, 1f);
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

		if (!limitAllControl && !rb.isKinematic)
			rb.linearVelocity = velocity;
		else if(!rb.isKinematic)
			rb.linearVelocity = Vector3.zero;

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
		foreach(Ability ability in abilities) { ability.UpdateTimers(); }
	}

	void RotatePlayer()
	{
		if (limitAllControl)
			return;

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
		if (cv.isLaunching ||dv.dashing || av.lv.leaping || wjv.wallJumpLimitVelocity || av.div.diving)
			limitVelocity = false;
		else
			limitVelocity = true;

		if (cv.isLaunching||dv.dashControlTimer > 0 || av.lv.leapControlTimer > 0 || wjv.wallJumpLimitVelocity || av.div.diving || lgv.ledgeGrabbing || av.gv.grinding)
			limitControl = true;
		else
			limitControl = false;

		if (lgv.ledgeGrabbing)
			limitAllControl = true;
		else
			limitAllControl = false;
	}

	public void HandleCookingStation(InputAction.CallbackContext context)
    {
		if (!gcv.grounded || jc.jumping || acv.falling || gcv.onSlope || jc.inAir || wjv.wallgrab || iv.interacting)
			return;

		velocity = Vector3.zero;
		rb.linearVelocity = Vector3.zero;

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
		animator.SetBool("Jumping", false);
		animator.SetBool("Running", false);
		animator.SetBool("Falling", false);
		animator.SetBool("Dashing", false);
		animator.SetBool("Grounded", false);
		animator.SetBool("Sliding", false);
	}
}