using NaughtyAttributes;
using UnityEngine;

public interface IMovement 
{
	public Rigidbody RB { get; set; }

	public Transform PlayerVisual { get; set; }
	public Animator PlayerAnimator { get; set; }

	public Vector2 LastPlayerInput { get; set; }
	public Vector3 DesiredVelocity { get; set; }
	public Vector3 Velocity { get; set; }
	public Vector3 LastInputDirection3D { get; set; }

	public GroundControlValues GCV { get; set; }
	public AirControlValues ACV { get; set; }
	public JumpingValues JC { get; set; }

	public WallJumpingValues WJV { get; set; }

	public DashingVariables DV { get; set; }

	public SwipingVariables SWV { get; set; }

	public GroundSurfVariables SUV { get; set; }

	public AdvancedMovement AV { get; set; }

	public InteractionVariables IV { get; set; }

	public Vector3 ProjectOnContactPlane(Vector3 vector);

	public void SetNextState(System.Type _type);

}
