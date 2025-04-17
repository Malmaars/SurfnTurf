using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public enum TargetBindingMode
{
	WorldSpace,
	TargetForward
}

[AddComponentMenu("Cinemachine/Body/Orbital Follow With Deadzone (Input System)")]
[ExecuteAlways]
[SaveDuringPlay]
public class OrbitalFollowWithDeadzone : CinemachineComponentBase
{
	[Header("Follow Target Settings")]
	public Vector3 FollowOffset = new Vector3(0, 1.5f, -5f);
	public TargetBindingMode BindingMode = TargetBindingMode.WorldSpace;

	[Header("Damping")]
	public Vector3 Damping = new Vector3(4f, 4f, 0f);

	[Header("Deadzone Settings (degrees)")]
	public float DeadzoneX = 5f;
	public float DeadzoneY = 5f;

	[Header("Vertical Axis Limits")]
	public float MinVerticalAngle = -30f;
	public float MaxVerticalAngle = 70f;

	[Header("Input System")]
	public bool EnableInput = true;

	[Tooltip("Input action for orbital movement (Vector2: X = horizontal, Y = vertical)")]
	public InputActionReference OrbitInput;

	public float InputSensitivityX = 100f;
	public float InputSensitivityY = 100f;
	public bool InvertX = false;
	public bool InvertY = false;

	private Vector2 _currentRotation;

	public override bool IsValid => true;
	public override CinemachineCore.Stage Stage => CinemachineCore.Stage.Body;

	public override void MutateCameraState(ref CameraState state, float deltaTime)
	{
		Vector3 targetPos = state.ReferenceLookAt;

		if (EnableInput && deltaTime > 0 && OrbitInput != null)
		{
			Vector2 input = OrbitInput.action?.ReadValue<Vector2>() ?? Vector2.zero;

			float inputX = input.x;
			float inputY = input.y;

			if (InvertX) inputX *= -1f;
			if (InvertY) inputY *= -1f;

			_currentRotation.y += inputX * InputSensitivityX * deltaTime;
			_currentRotation.x += inputY * InputSensitivityY * deltaTime;
		}

		_currentRotation.x = Mathf.Clamp(_currentRotation.x, MinVerticalAngle, MaxVerticalAngle);

		Vector3 toTarget = targetPos - state.RawPosition;
		if (toTarget.sqrMagnitude > 0.01f)
		{
			Quaternion desiredRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
			Vector3 desiredAngles = desiredRot.eulerAngles;

			float desiredPitch = NormalizeAngle(desiredAngles.x);
			float desiredYaw = NormalizeAngle(desiredAngles.y);

			float deltaYaw = Mathf.DeltaAngle(_currentRotation.y, desiredYaw);
			float deltaPitch = Mathf.DeltaAngle(_currentRotation.x, desiredPitch);

			if (Mathf.Abs(deltaYaw) > DeadzoneX)
				_currentRotation.y += deltaYaw * Mathf.Clamp01(deltaTime * Damping.y);

			if (Mathf.Abs(deltaPitch) > DeadzoneY)
				_currentRotation.x += deltaPitch * Mathf.Clamp01(deltaTime * Damping.x);
		}

		Quaternion finalRot = Quaternion.Euler(_currentRotation.x, _currentRotation.y, 0f);
		Quaternion targetRot = GetTargetOrientation(state, BindingMode, Vector3.up);
		Vector3 offset = finalRot * FollowOffset;
		Vector3 desiredPos = targetPos + offset;

		state.RawPosition = Vector3.Lerp(state.RawPosition, desiredPos, 1 - Mathf.Exp(-Damping.z * deltaTime));
		state.RawOrientation = Quaternion.LookRotation(targetPos - state.RawPosition, Vector3.up);
	}

	private float NormalizeAngle(float angle)
	{
		angle %= 360f;
		if (angle > 180f) angle -= 360f;
		return angle;
	}

	private Quaternion GetTargetOrientation(CameraState state, TargetBindingMode mode, Vector3 up)
	{
		switch (mode)
		{
			case TargetBindingMode.WorldSpace:
				return Quaternion.identity;
			case TargetBindingMode.TargetForward:
				return Quaternion.LookRotation(state.ReferenceLookAt - state.RawPosition, up);
			default:
				return Quaternion.identity;
		}
	}
}
