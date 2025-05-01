using System;
using Unity.VisualScripting;
using UnityEngine;

public class Swim : WaterAbility
{
	public Swim(WaterMovementController _mov) : base(_mov) { }

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSwim();
	}

	void HandleSwim()
	{
		if (mov.wv.onWater && !mov.suv.surfing)
		{
			if (!mov.sv.swimming)
				mov.sv.swimmingAnimation = true;
			mov.sv.swimming = true;
		}
		else
			mov.sv.swimming = false;

		if (mov.sv.swimming)
			DoSwim();
	}

	public override void ResetValues()
	{
		mov.sv.swimming = false;
	}

	void DoSwim()
	{
		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
		if (mov.iv.interacting)
			playerInput = Vector2.zero;

		float inputMagnitude = 1;
		playerInput = new Vector2(MathF.Round(playerInput.x), MathF.Round(playerInput.y));
		playerInput = playerInput.normalized * inputMagnitude;

		Vector3 cameraDirection = Camera.main.transform.forward;
		cameraDirection.y = 0;
		Vector3 cameraRightDirection = Camera.main.transform.right;
		cameraRightDirection.y = 0;
		Vector3 newMovementVector = mov.ProjectOnContactPlane((cameraDirection * playerInput.y) + cameraRightDirection * playerInput.x);

		if (playerInput != Vector2.zero)
			mov.lastInputDirection3D = newMovementVector.normalized;

		float maxSpeed = mov.sv.maxSpeed;

		newMovementVector = newMovementVector.normalized * playerInput.magnitude;
		mov.desiredVelocity = newMovementVector * maxSpeed;

		float acceleration = mov.sv.maxAcceleration;
		if (!mov.wv.onWater)
			acceleration = 0;

		float maxSpeedChange = acceleration * Time.deltaTime;
		mov.velocity = Vector3.MoveTowards(mov.velocity, mov.desiredVelocity, maxSpeedChange);
	}

	public override void UpdateAnimator()
	{
		if(mov.sv.swimmingAnimation)
		{
			mov.animator.SetTrigger("IntoWater");
			mov.sv.swimmingAnimation = false;
		}

		mov.animator.SetBool("Water", mov.wv.onWater);

		Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();
		mov.animator.SetBool("Swimming", (mov.sv.swimming && playerInput != Vector2.zero));
	}
}
