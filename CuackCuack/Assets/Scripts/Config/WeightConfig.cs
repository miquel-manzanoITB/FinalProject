using UnityEngine;

/// <summary>
/// Shared configuration that maps object weight to gameplay modifiers.
/// Create one asset via Assets > Create > Game > Weight Config
/// and assign it to PlayerInteraction and PlayerMovement.
/// </summary>
[CreateAssetMenu(menuName = "Game/Weight Config", fileName = "WeightConfig")]
public class WeightConfig : ScriptableObject
{
    [Header("Drag Force")]
    [Tooltip("How much force is applied to pull an object. Evaluated against object weight (0..maxWeight).")]
    public AnimationCurve dragForceCurve = AnimationCurve.Linear(0f, 80f, 50f, 20f);

    [Header("Player Speed Penalty While Dragging")]
    [Tooltip("Speed multiplier applied to the player while dragging. 1 = no penalty. Evaluated against object weight.")]
    public AnimationCurve speedPenaltyCurve = AnimationCurve.Linear(0f, 1f, 50f, 0.3f);

    [Header("Object Damping")]
    [Tooltip("LinearDamping applied to the object while being dragged. Higher = more sluggish.")]
    public AnimationCurve dragDampingCurve = AnimationCurve.Linear(0f, 8f, 50f, 20f);

    [Header("Weight Range")]
    public float maxWeight = 50f;

    // ── Evaluators ────────────────────────────────────────────────────────────

    public float GetDragForce(float weight) => dragForceCurve.Evaluate(weight);
    public float GetSpeedPenalty(float weight) => speedPenaltyCurve.Evaluate(weight);
    public float GetDragDamping(float weight) => dragDampingCurve.Evaluate(weight);
}
