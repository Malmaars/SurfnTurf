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
	[Label("Spindash (Swipe -> Dash)")]
	[AllowNesting]
	public SpindashVariables spd;
	[Label("Diving (Dash -> Swipe)")]
	[AllowNesting]
	public DiveVariables div;
	[Label("TwirlSurfing (Swipe -> Surf)")]
	[AllowNesting]
	public TwirlSurfVariables tsv;
	[Label("Sliding (Surf -> Dash)")]
	[AllowNesting]
	public SlideVariables slv;
	[Label("Board Tricks (Surf -> Swipe or Jump)")]
	[AllowNesting]
	public BoardtrickVariables btv;
}