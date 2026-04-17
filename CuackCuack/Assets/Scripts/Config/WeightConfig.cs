using UnityEngine;

/// <summary>
/// Shared configuration that maps object weight to gameplay modifiers.
/// Curves inspired by REPO-style feel: snappy on light objects, sluggish on heavy ones.
/// Create one asset via Assets > Create > Game > Weight Config
/// and assign it to PlayerInteraction and PlayerMovement.
/// </summary>
[CreateAssetMenu(menuName = "Game/Weight Config", fileName = "WeightConfig")]
public class WeightConfig : ScriptableObject
{
    [Header("Weight Range")]
    [Tooltip("Max weight in kg that the curves evaluate against.")]
    public float maxWeight = 50f;

    // ── Drag Force ────────────────────────────────────────────────────────────
    [Header("Drag Force")]
    [Tooltip(
        "Force applied to pull the object toward the target point.\n" +
        "Light objects (0 kg) -> high force (snappy). Heavy objects -> low force (sluggish).\n" +
        "Recommended shape: starts high (~120), drops fast in the first 10 kg, then flattens toward ~15 at max weight.\n" +
        "Use EaseIn or custom keys -- avoid Linear to feel more like REPO."
    )]
    public AnimationCurve dragForceCurve = DefaultDragForceCurve();

    // ── Speed Penalty ─────────────────────────────────────────────────────────
    [Header("Player Speed Penalty While Dragging")]
    [Tooltip(
        "Speed multiplier applied to the player while dragging (1 = no penalty).\n" +
        "Light objects barely slow you down. Heavy objects cut speed significantly.\n" +
        "Recommended shape: stays near 1.0 until ~10 kg, then drops steeply to ~0.35 by max weight."
    )]
    public AnimationCurve speedPenaltyCurve = DefaultSpeedPenaltyCurve();

    // ── Object Damping ────────────────────────────────────────────────────────
    [Header("Object Damping While Dragging")]
    [Tooltip(
        "LinearDamping applied to the object while held.\n" +
        "Light objects are lively (low damping). Heavy objects resist movement (high damping).\n" +
        "Recommended shape: starts at 5, rises quickly to ~18 by 20 kg, then eases to ~25."
    )]
    public AnimationCurve dragDampingCurve = DefaultDragDampingCurve();

    // ── Gravity Scale ─────────────────────────────────────────────────────────
    [Header("Gravity Scale (FREE objects, not held)")]
    [Tooltip(
        "Extra gravity multiplier applied to the Rigidbody when the object is NOT being held.\n" +
        "Unity's default gravity scale is 1. Values above 1 make objects fall faster.\n" +
        "Light objects (feathers, papers) -> ~1.0. Heavy objects (metal box) -> ~3.0.\n" +
        "Recommended shape: flat near 1.0 up to ~5 kg, then rises to 2.5-3.0 at max weight."
    )]
    public AnimationCurve gravityScaleCurve = DefaultGravityScaleCurve();

    // ── Collision Impulse ─────────────────────────────────────────────────────
    [Header("Collision Impulse (heavy hitting light)")]
    [Tooltip(
        "Extra impulse magnitude added to the lighter object when a heavier object collides with it.\n" +
        "This is evaluated against the MASS DIFFERENCE between the two objects (0..maxWeight).\n" +
        "Small difference -> tiny nudge (~0). Large difference -> big launch (~25).\n" +
        "Recommended shape: near 0 until 10 kg difference, then exponential rise."
    )]
    public AnimationCurve collisionImpulseCurve = DefaultCollisionImpulseCurve();

    // ── Evaluators ────────────────────────────────────────────────────────────

    public float GetDragForce(float weight) => dragForceCurve.Evaluate(weight);
    public float GetSpeedPenalty(float weight) => speedPenaltyCurve.Evaluate(weight);
    public float GetDragDamping(float weight) => dragDampingCurve.Evaluate(weight);
    public float GetGravityScale(float weight) => gravityScaleCurve.Evaluate(weight);
    public float GetCollisionImpulse(float massDiff) => collisionImpulseCurve.Evaluate(massDiff);

    // ── Default Curve Factories ───────────────────────────────────────────────
    // These give a good REPO-like starting point. Tweak keys in the Inspector.

    private static AnimationCurve DefaultDragForceCurve()
    {
        // Fast drop from 120 -> 60 in first 5 kg, slow ease to 15 at 50 kg
        var c = new AnimationCurve();
        c.AddKey(new Keyframe(0f, 120f, 0f, -20f));
        c.AddKey(new Keyframe(5f, 60f, -8f, -2f));
        c.AddKey(new Keyframe(20f, 28f, -1f, -0.5f));
        c.AddKey(new Keyframe(50f, 15f, 0f, 0f));
        return c;
    }

    private static AnimationCurve DefaultSpeedPenaltyCurve()
    {
        // Stays near 1.0 for light objects, drops hard after 10 kg
        var c = new AnimationCurve();
        c.AddKey(new Keyframe(0f, 1.00f, 0f, -0.01f));
        c.AddKey(new Keyframe(10f, 0.85f, -0.02f, -0.04f));
        c.AddKey(new Keyframe(25f, 0.55f, -0.02f, -0.015f));
        c.AddKey(new Keyframe(50f, 0.30f, 0f, 0f));
        return c;
    }

    private static AnimationCurve DefaultDragDampingCurve()
    {
        // Starts lively, rises fast then plateaus
        var c = new AnimationCurve();
        c.AddKey(new Keyframe(0f, 5f, 0f, 2f));
        c.AddKey(new Keyframe(10f, 14f, 1.5f, 0.8f));
        c.AddKey(new Keyframe(25f, 20f, 0.3f, 0.2f));
        c.AddKey(new Keyframe(50f, 25f, 0f, 0f));
        return c;
    }

    private static AnimationCurve DefaultGravityScaleCurve()
    {
        // Near 1 for light objects, rises to 3 for heavy -- makes heavy things fall fast
        var c = new AnimationCurve();
        c.AddKey(new Keyframe(0f, 1.0f, 0f, 0.01f));
        c.AddKey(new Keyframe(5f, 1.1f, 0.02f, 0.05f));
        c.AddKey(new Keyframe(20f, 1.9f, 0.08f, 0.06f));
        c.AddKey(new Keyframe(50f, 3.0f, 0f, 0f));
        return c;
    }

    private static AnimationCurve DefaultCollisionImpulseCurve()
    {
        // Small mass difference -> negligible push. Big difference -> send it flying
        var c = new AnimationCurve();
        c.AddKey(new Keyframe(0f, 0f, 0f, 0f));
        c.AddKey(new Keyframe(10f, 2f, 0.3f, 1f));
        c.AddKey(new Keyframe(25f, 12f, 1f, 1.5f));
        c.AddKey(new Keyframe(50f, 28f, 0f, 0f));
        return c;
    }
}
