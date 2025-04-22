using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class WaterAbility
{
	protected WaterMovementController mov;
	public WaterAbility(WaterMovementController _mov)
	{
		mov = _mov;
	}


	public virtual void RunOnExitState() { }
	public virtual void RunOnEnterState() { }
	public virtual void RunOnAwake() { }
	public virtual void RunOnStart() { }
	public virtual void RunOnDrawGizmos() { }
	public virtual void RunOnUpdateBeforeSetVelocity() { }
	public virtual void RunOnUpdateDuringSetVelocity() { }
	public virtual void RunOnUpdateAfterSetVelocity() { }

	public virtual void UpdateTimers() { }

	public virtual void ResetValues() { }

	public virtual void UpdateAnimator() { }
	public virtual void OnInput(InputAction.CallbackContext context) { }
}
