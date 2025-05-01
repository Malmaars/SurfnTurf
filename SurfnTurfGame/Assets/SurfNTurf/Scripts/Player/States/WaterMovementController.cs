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
public class WaterMovementController : PlayerState
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

	WaterAbility[] abilities;

	[Header("State settings")]
	public bool enableCookingStation;

	[Label("Interacting")]
	public InteractionVariables iv;
	
	[Label("Base Water Variables")]
	public WaterVariables wv;

	[Label("Water Jump")]
	public WaterJumpVariables wj;

	[Label("Water Swipe")]
	public WaterSwipeVariables ws;

	[Label("Swimming")]
	public SwimmingVariables sv;

	[Label("Air Control")]
	public WaterAirControlVariables acv;

	[Label("Surfing")]
	public WaterSurfingVariables suv;
	
	[Label("Tricks")]
	public WaterTricksVariables wtv;

	private void OnValidate() { }

	private void Awake()
	{
		abilities = new WaterAbility[] {
			new WaterChecker(this),
			new WaterAirControl(this),
			new Swim(this),
			new WaterSurf(this),
			new WaterJump(this),
			new WaterSwipe(this),
			new Tricks(this)
		};
		foreach (WaterAbility ability in abilities)
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
	}
	private void Start()
	{
		rb = GetComponentInChildren<Rigidbody>();
		foreach (WaterAbility ability in abilities) { ability.RunOnStart(); }
	}

	public override void InitStateTransitions()
	{
		base.InitStateTransitions();
		transitions.Add(new PlayerStateTransition(typeof(PauseState), () => nextState == typeof(PauseState)));
		transitions.Add(new PlayerStateTransition(typeof(InventoryMenuManager), () => nextState == typeof(InventoryMenuManager)));
		transitions.Add(new PlayerStateTransition(typeof(MovementController), () => nextState == typeof(MovementController), new specialExit[] { ResetAnimator }));
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

		foreach (WaterAbility ability in abilities) { ability.RunOnEnterState(); }

		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, OpenInventoryMenu);
		BlackBoard.cameraController.SwitchToCamera(playerCam, 0.5f);
		base.EnterState();
	}

	public override void ExitState()
	{
		foreach (WaterAbility ability in abilities) { ability.RunOnExitState(); }

		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, OpenInventoryMenu);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Menu.Pause, PauseGame);
		PlayerVFX.instance.runningDust.SendEvent("OnStop");
		base.ExitState();
	}

	public void SetNextState(System.Type _type)
	{
		nextState = _type;
	}

	void AdjustVelocity()
	{
		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
		if (iv.interacting)
			playerInput = Vector2.zero;

			playerInput = Vector2.ClampMagnitude(playerInput, 1f);
		
		Vector3 cameraDirection = Camera.main.transform.forward;
		cameraDirection.y = 0;
		Vector3 cameraRightDirection = Camera.main.transform.right;
		cameraRightDirection.y = 0;
		Vector3 newMovementVector = ProjectOnContactPlane((cameraDirection * playerInput.y) + cameraRightDirection * playerInput.x);

		if (playerInput != Vector2.zero)
			lastInputDirection3D = newMovementVector.normalized;
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


	public void ResetValues()
	{
		foreach (WaterAbility ability in abilities) { ability.ResetValues(); }
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
		wv.allContactNormals = new Vector3[collision.contactCount];
		for (int i = 0; i < collision.contactCount; i++)
		{

			Vector3 normal = collision.GetContact(i).normal;
			wv.allContactNormals[i] = normal;
				wv.waterContactCount++;
				wv.contactNormal += normal;
				if (((1 << collision.GetContact(i).otherCollider.gameObject.layer) & waterlayers) == 0)
					nextState = typeof(MovementController);
		}
		if (wv.waterContactCount > 1)
			wv.contactNormal.Normalize();
		else if (wv.waterContactCount == 0)
			wv.contactNormal = Vector3.zero;
	}

	public Vector3 ProjectOnContactPlane(Vector3 vector)
	{
		return vector - wv.contactNormal * Vector3.Dot(vector, wv.contactNormal);
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

		foreach (WaterAbility ability in abilities)
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

		AdjustVelocity();

		velocity = rb.linearVelocity;
		foreach (WaterAbility ability in abilities)
		{
			ability.RunOnUpdateBeforeSetVelocity();
		}
		UpdateTimers();

		foreach (WaterAbility ability in abilities)
		{
			ability.RunOnUpdateDuringSetVelocity();
		}

		RotatePlayer();

		HandleLimiter();

		if (!limitAllControl)
			rb.linearVelocity = velocity;
		else if (!rb.isKinematic)
			rb.linearVelocity = Vector3.zero;

		foreach (WaterAbility ability in abilities)
		{
			ability.RunOnUpdateAfterSetVelocity();
		}

		UpdateAnimator();
	}

	private void FixedUpdate()
	{
		wv.waterContactCount= 0;
		wv.contactNormal = Vector3.zero;
	}


	void UpdateTimers()
	{
		foreach (WaterAbility ability in abilities) { ability.UpdateTimers(); }
	}

	void RotatePlayer()
	{
		if (limitAllControl)
			return;

		float rotationSpeed = visualRotationSpeed;

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

	}

	public void OpenInventoryMenu(InputAction.CallbackContext context)
	{
		if (iv.interacting)
			return;
		velocity = Vector3.zero;
		rb.linearVelocity = Vector3.zero;
		nextState = typeof(InventoryMenuManager);
	}

	void UpdateAnimator()
	{
		foreach (WaterAbility ability in abilities)
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