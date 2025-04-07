using NaughtyAttributes;

[System.Serializable]
public class AdvancedMovement
{
	[Label("Leaping (Dash -> Jump)")]
	[AllowNesting]
	public LeapingVariables lv;
	[Label("Twirl Jump (Swipe -> Jump)")]
	[AllowNesting]
	public TwirlJumpVariables tj;
	[Label("Swipe double Jump (Jump -> Swipe)")]
	[AllowNesting]
	public SwipeDoubleJumpVariables sdj;
	[Label("Surf Ground Parry")]
	[AllowNesting]
	public SurfParryVariables sp;
}