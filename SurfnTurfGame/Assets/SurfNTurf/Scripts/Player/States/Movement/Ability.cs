using System;
using UnityEngine;
using UnityEngine.InputSystem;

[Serializable]
public class Ability
{
	protected IMovement mov;
	public Ability(IMovement _mov)
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

	public virtual void UpdateAnimator() { }
	public virtual void OnInput(InputAction.CallbackContext context) { }
}
