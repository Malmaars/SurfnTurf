using NaughtyAttributes;
using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class WaterMovementController : PlayerState
{
	public bool gizmosOn;
	public CinemachineCamera playerCam;

	Rigidbody rb;

	public LayerMask waterlayers;

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

	[Label("On Water Control")]
	public WaterControlValues wcv;
	[Label("Air Control")]
	public WaterAirControlValues wacv;
	[Label("Jumping")]
	public WaterJumpingValues wjc;

	[AnimatorParam(nameof(animator))]
	public string surfingName;

	private void OnValidate()
	{
		wcv.minGroundDotProduct = Mathf.Cos(wcv.maxGroundAngle * Mathf.Deg2Rad);
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
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, OpenInventoryMenu);
	}
	public override void EnterState()
	{
		Debug.Log("Entering Water Move State");
		ResetValues();
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
		InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Jump, EndJump);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.OpenInventoryMenu, OpenInventoryMenu);

		BlackBoard.cameraController.SwitchToCamera(playerCam);

		animator.SetBool(surfingName, true);

		base.EnterState();
	}

	public override void ExitState()
	{
		animator.SetBool(surfingName, false);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Jump, StartJump);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Jump, EndJump);
		base.ExitState();
	}
	void ResetValues()
	{
		wacv.falling = false;
		wjc.desiredJump = false;
		wjc.jumping = false;
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
		for (int i = 0; i < collision.contactCount; i++)
		{
			Vector3 normal = collision.GetContact(i).normal;
			if (normal.y >= wcv.minGroundDotProduct)
			{
				wcv.groundContactCount++;
				wcv.contactNormal += normal;

                if (((1 << collision.GetContact(i).otherCollider.gameObject.layer) & waterlayers) == 0)
					BlackBoard.playerManager.SwitchState(typeof(MovementController));
			}
		}
		if (wcv.groundContactCount > 1)
			wcv.contactNormal.Normalize();
		else if (wcv.groundContactCount == 0)
			wcv.contactNormal = Vector3.zero;
	}

	Vector3 ProjectOnContactPlane(Vector3 vector)
	{
		return vector - wcv.contactNormal * Vector3.Dot(vector, wcv.contactNormal);
	}

	void AdjustVelocity()
	{
		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();

		if (wcv.eightWayDirectionInput)
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

		newMovementVector = newMovementVector.normalized * playerInput.magnitude;
		desiredVelocity = newMovementVector * wcv.maxSpeed;

		float acceleration = wcv.grounded ? wcv.maxAcceleration : wacv.maxAirAcceleration;

		float maxSpeedChange = acceleration * Time.deltaTime;



		if (wacv.antiAirTimer <= 0)
		{
			if (wcv.grounded && !wjc.jumping)
				velocity = Vector3.MoveTowards(velocity, desiredVelocity, maxSpeedChange);
			else
				velocity = Vector3.MoveTowards(velocity, new Vector3(desiredVelocity.x, velocity.y, desiredVelocity.z), maxSpeedChange);
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

			Gizmos.color = Color.blue;

			Gizmos.DrawLine(rb.position, rb.position + velocity);

			Gizmos.color = Color.red;
			Vector3 gradient;

			gradient = ProjectOnContactPlane(Vector3.down);
			Gizmos.DrawLine(rb.position, rb.position + gradient.normalized * 3);
			Gizmos.DrawLine(rb.position, rb.position + Vector3.down * wcv.groundSnapProbeDistance);

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
					desiredVelocity = newMovementVector * wcv.maxSpeed;

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
					desiredVelocity = newMovementVector * wcv.maxSpeed;

					Gizmos.DrawLine(rb.position, rb.position + desiredVelocity.normalized * 3);
				}
			}
		}
	}
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.P))
		{
			Time.timeScale = 0.1f;
		}

		velocity = rb.linearVelocity;
		UpdateTimers();
		CheckGrounded();
		UpdateGroundedValues();
		CheckFalling();
		AdjustVelocity();
		AddGravity();
		HandleJumping();
		CheckLanding();
		RotatePlayer();
		rb.linearVelocity = velocity;
		UpdateAnimator();
	}

	private void FixedUpdate()
	{
		wcv.groundContactCount = 0;
		wcv.contactNormal = Vector3.zero;
	}

	void UpdateGroundedValues()
	{
		if (wcv.grounded)
		{
			wacv.antiAirTimer = 0;
			wjc.inAir = false;
			wjc.coyoteTimeAvailable = true;

			if (!wjc.jumping)
				wjc.jumpPhase = 0;
		}

		else
		{
			wcv.contactNormal = Vector3.zero;

			if (!wjc.inAir)
			{
				wjc.jumpPhase = 1;
				wjc.inAir = true;
			}
		}
	}
	void UpdateTimers()
	{
		if (wjc.jumpBufferTimer > 0)
			wjc.jumpBufferTimer -= Time.deltaTime;
		if (wjc.coyoteTimer > 0)
			wjc.coyoteTimer -= Time.deltaTime;
		if (wacv.antiAirTimer > 0)
			wacv.antiAirTimer -= Time.deltaTime;
	}

	void CheckFalling()
	{
		if (!wcv.grounded && wjc.inAir && !wjc.jumping && !Physics.Raycast(rb.position, Vector3.down, wcv.groundSnapProbeDistance))
		{
			wacv.falling = true;
			if (wjc.coyoteTimeAvailable)
			{
				wjc.coyoteTimer = wjc.coyoteTime;
				wjc.coyoteTimeAvailable = false;
			}
		}
	}
	void CheckGrounded()
	{
		if (wcv.groundContactCount > 0)
			wcv.grounded = true;
		else
			wcv.grounded = false;
	}

	void HandleJumping()
	{
		if (wcv.grounded && !wjc.jumping)
			wjc.jumpPhase = 0;

		if (!wcv.grounded && wjc.jumping)
			wjc.coyoteTimeAvailable = false;

		if (wjc.desiredJump || (wcv.grounded && wjc.jumpBufferTimer > 0))
		{
			wjc.desiredJump = false;
			Jump();
		}

		if (wjc.jumping && velocity.y < 0f)
			wjc.jumping = false;
	}
	void Jump()
	{
		if (wcv.grounded || wjc.jumpPhase <= wjc.maxAirJumps || wjc.coyoteTimer > 0)
		{
			wjc.jumpBufferTimer = 0;
			wjc.jumping = true;

			float jumpSpeed = wjc.jumpHeight;
			//float alignedSpeed = Vector3.Dot(velocity, contactNormal);

			//if (alignedSpeed > 0f)
			//{
			//    jumpSpeed = Mathf.Max(jumpSpeed - alignedSpeed, 0f);
			//}

				velocity.y = 0;
					
				velocity += wcv.contactNormal * jumpSpeed;

				if (jumpSpeed > 0f)
					wjc.jumpPhase++;

			if (wjc.coyoteTimer > 0)
				wjc.jumpPhase = 1;

			wjc.coyoteTimeAvailable = false;
			wjc.coyoteTimer = 0;
		}
	}

	void RotatePlayer()
	{
		float rotationSpeed = visualRotationSpeed;
		if (new Vector3(velocity.x, 0, velocity.z).sqrMagnitude > 0.01f && new Vector3(velocity.x, 0, velocity.z) != Vector3.zero && playerVisual.forward != new Vector3(velocity.x, 0, velocity.z))
		{
			Quaternion newRotation = Quaternion.LookRotation(new Vector3(velocity.x, 0, velocity.z));

			playerVisual.localRotation = Quaternion.Slerp
			   (playerVisual.localRotation, newRotation, visualRotationSpeed * Time.deltaTime);
		}
	}

	void AddGravity()
	{
		if (wcv.grounded == true)
			return;

		if (rb.linearVelocity.y > wacv.maximumDownVelocity)
		{
			//apply a consistent downforce, perhaps greater than normal gravity
				rb.AddForce(Vector3.up * wacv.customGravityStrength * Time.deltaTime * 100);
		}
	}

	void CheckLanding()
	{

		if (wcv.grounded && !wjc.jumping && !wjc.hasLanded && wacv.falling)
		{
			wjc.hasLanded = true;
			wjc.hasLandedAnimation = true;
			wacv.falling = false;
		}
		if (!wcv.grounded)
			wjc.hasLanded = false;
	}
	public void StartJump(InputAction.CallbackContext context)
	{
		if (!wjc.jumpingOn)
			return;

		wjc.desiredJump = true;
		wjc.jumpBufferTimer = wjc.jumpBufferTime;
	}

	public void EndJump(InputAction.CallbackContext context)
	{
        if (!wjc.jumpingOn)
            return;

        wjc.desiredJump = false;
	}

	public void OpenInventoryMenu(InputAction.CallbackContext context)
	{
		BlackBoard.playerManager.SwitchState(typeof(InventoryMenuManager));
	}

	void UpdateAnimator()
	{
		animator.SetFloat("Speed", new Vector2(velocity.x, velocity.z).magnitude / 10);

		if (wjc.jumping != animator.GetBool("Jumping"))
			animator.SetBool("Jumping", wjc.jumping);

		if ((!wcv.grounded && !wjc.jumping && wacv.falling) != animator.GetBool("Falling"))
			animator.SetBool("Falling", ((!wcv.grounded && !wjc.jumping && wacv.falling)));

		if (wcv.grounded == true)
		{
			if (wjc.hasLandedAnimation == true)
			{
				animator.SetTrigger("Landing");
				wjc.hasLandedAnimation = false;
			}
		}
		else
			wjc.hasLandedAnimation = false;
	}
}

[System.Serializable]
public class WaterControlValues
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

	public LayerMask groundedLayerMask;

	[ReadOnly]
	[AllowNesting]
	public int groundContactCount;

	[ReadOnly]
	[AllowNesting]
	public bool grounded;

	[SerializeField, Range(0f, 100f)]
	public float maxSnapSpeed = 100f;

	[SerializeField, Min(0f)]
	public float groundSnapProbeDistance = 1f;

	[ReadOnly]
	[AllowNesting]
	public Vector3 contactNormal;
}

[System.Serializable]
public class WaterAirControlValues
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
public class WaterJumpingValues
{
	public bool jumpingOn;

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
