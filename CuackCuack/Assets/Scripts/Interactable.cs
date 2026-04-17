using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Settings")]
    public bool isDraggable = true;

    [Header("Weight")]
    [Tooltip("Object weight in kg. Affects drag force, player speed, damping, gravity and collision impulse.")]
    public float weight = 5f;

    [Header("Events")]
    public UnityEvent onInteract;
    public UnityEvent onPickUp;

    // ── Internal ──────────────────────────────────────────────────────────────

    private Rigidbody _rb;
    private WeightConfig _weightConfig;   // injected by PlayerInteraction on first drag
    private bool _isBeingHeld;
    private float _defaultGravityScale = 1f;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();

        if (_rb != null)
            _defaultGravityScale = _rb.useGravity ? 1f : 0f;
    }

    /// <summary>
    /// Called once by PlayerInteraction so the object knows about the config.
    /// Done lazily to avoid requiring a direct Inspector link on every object.
    /// </summary>
    public void InjectWeightConfig(WeightConfig config)
    {
        _weightConfig = config;
        ApplyGravityScale(); // set correct gravity now that we have the config
    }

    // ── Drag API (called by PlayerInteraction) ────────────────────────────────

    public void StartDrag(float damping)
    {
        if (_rb == null) return;
        _isBeingHeld = true;
        _rb.linearDamping = damping;
        // While held, use normal gravity so the object doesn't rocket upward
        _rb.useGravity = true;
        //_rb.gravityScale = 1f;
    }

    public void DragTowards(Vector3 targetPos, float force)
    {
        if (!isDraggable || _rb == null) return;
        Vector3 direction = targetPos - transform.position;
        _rb.AddForce(direction * force, ForceMode.Force);
    }

    public void StopDrag()
    {
        if (_rb == null) return;
        _isBeingHeld = false;
        _rb.linearDamping = 1f;
        ApplyGravityScale(); // restore weight-based gravity
    }

    // ── Collision ─────────────────────────────────────────────────────────────

    void OnCollisionEnter(Collision collision)
    {
        if (_weightConfig == null) return;

        // Only act when a HEAVIER object hits a LIGHTER one
        var other = collision.collider.GetComponent<Interactable>();
        if (other == null) return;

        float massDiff = weight - other.weight;
        if (massDiff <= 0f) return; // we are not heavier, skip

        float impulse = _weightConfig.GetCollisionImpulse(massDiff);
        if (impulse <= 0f) return;

        // Launch the lighter object away from us
        Vector3 dir = (other.transform.position - transform.position).normalized;
        // Slightly upward bias so it feels dramatic
        dir = (dir + Vector3.up * 0.3f).normalized;

        Rigidbody otherRb = other.GetComponent<Rigidbody>();
        if (otherRb != null)
            otherRb.AddForce(dir * impulse, ForceMode.Impulse);
    }

    // ── Rotation ──────────────────────────────────────────────────────────────

    public void ApplyRotation(Vector2 mouseDelta, Transform cameraTransform, float sensitivity)
    {
        if (_rb == null) return;
        _rb.MoveRotation(_rb.rotation
            * Quaternion.AngleAxis(mouseDelta.x * sensitivity, cameraTransform.up)
            * Quaternion.AngleAxis(-mouseDelta.y * sensitivity, cameraTransform.right));
    }

    // ── Events ────────────────────────────────────────────────────────────────

    public void Interact() => onInteract?.Invoke();
    public void PickUp() => onPickUp?.Invoke();

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Sets the Rigidbody gravityScale according to the object's weight.
    /// Called when the object is dropped / spawned.
    /// </summary>
    private void ApplyGravityScale()
    {
        if (_rb == null || _weightConfig == null) return;
        _rb.useGravity = true;
        //_rb.gravityScale = _weightConfig.GetGravityScale(weight);
    }
}
