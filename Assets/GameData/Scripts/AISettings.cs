using UnityEngine;

public class AISettings : ScriptableObject
{
	[Header("Difficulty curves")]
	[Tooltip("Variance percentage applied to all curves")]
	public float curveVarianceMax;

	public AnimationCurve closeToTargetPrecisionCurve;

	public AnimationCurve shootReactionCurve;

	public AnimationCurve afterShootDelayCurve;

	public AnimationCurve boosterLocationPrecisionCurve;

	[Header("Curve min max values")]
	public MinMaxFloat shootReactionDelay;

	public MinMaxFloat afterShootDelay;

	public MinMaxFloat useBoosterTargetRadius;

	[Header("Behaviour profiles")]
	public bool startAsRandomProfile;

	public AIBehaviour.Behaviour startAsProfile;

	public AIProfileSettings[] profiles;
}
