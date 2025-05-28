using SurfnTurf;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class WaterSurf : WaterAbility
{
	public WaterSurf(WaterMovementController _mov) : base(_mov) { }
	public override void RunOnEnterState()
	{
		InputDistributor.inputManager.AddActionToInput(InputDistributor.playerInputActions.Movement.Surf, StartSurf);
		InputDistributor.inputManager.AddActionToInputCancelled(InputDistributor.playerInputActions.Movement.Surf, EndSurf);

		//check if surfing is true
		if (InputDistributor.playerInputActions.Movement.Surf.IsPressed())
		{
			DoSurf();
		}

	}

	public override void RunOnExitState()
	{
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Surf, StartSurf);
		InputDistributor.inputManager.RemoveActionFromInput(InputDistributor.playerInputActions.Movement.Surf, EndSurf);
	}

	public override void RunOnUpdateDuringSetVelocity()
	{
		HandleSurfing();
	}
	void StartSurf(InputAction.CallbackContext context)
	{
		if (mov.suv.surfCooldownTimer > 0)
			return;

		mov.suv.desiredSurf = true;
		mov.suv.startSurfBufferTimer = mov.suv.startSurfBuffer;
	}

	void EndSurf(InputAction.CallbackContext context)
	{
		if (!mov.suv.surfing)
			return;

		mov.suv.desiredSurf = false;
		mov.suv.surfing = false;
		mov.suv.surfCooldownTimer = mov.suv.surfCooldown;
	}

	public override void UpdateTimers()
	{
		mov.suv.surfCooldownTimer = mov.suv.surfCooldownTimer.TimerCountdown();
		mov.suv.startSurfBufferTimer = mov.suv.startSurfBufferTimer.TimerCountdown();
	}

	void HandleSurfing()
	{
		if (mov.suv.desiredSurf)
		{
			DoSurf();
			mov.suv.desiredSurf = false;
		}

		else if (mov.suv.surfing && mov.wv.onWater && !mov.wrv.onWave && !mov.gv.grinding)
		{
			float maxSpeed = mov.wv.comboAddsSpeed ? mov.suv.maxSurfSpeed + ComboMeter.currentCombo : mov.suv.maxSurfSpeed;
			if (mov.velocity.magnitude < maxSpeed)
			{
				mov.velocity += mov.playerVisual.forward * mov.suv.accelarationSpeed * Time.deltaTime;
			}

			else if (mov.velocity.magnitude > maxSpeed)
			{
				//slow the player
				mov.velocity -= mov.playerVisual.forward * mov.suv.decelerationSpeed * Time.deltaTime;
			}
				float velocityMag = mov.velocity.magnitude;
				//slightly change the angle of the surf
				Vector2 playerInput = InputDistributor.playerInputActions.Movement.DirectionalInput.ReadValue<Vector2>();

				if (playerInput != Vector2.zero)
					mov.velocity = (mov.velocity.normalized + (mov.lastInputDirection3D * mov.suv.steeringStrength * Time.deltaTime)).normalized * velocityMag;
		}
	}



	void DoSurf()
	{
		mov.suv.surfing = true;
	}

	public override void ResetValues()
	{
		mov.suv.surfing = false;
	}

	public void AnglePlayer()
	{

	}
	public override void UpdateAnimator()
	{
		if (mov.suv.surfing && !mov.animator.GetBool("Surfing"))
		{
			mov.animator.SetTrigger("Surf");
		}
		mov.animator.SetBool("Surfing", mov.suv.surfing);
		SurfBoardManager.instance.ToggleSurfboard(mov.suv.surfing);
	}
}
