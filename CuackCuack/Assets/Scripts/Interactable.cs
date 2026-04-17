using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Settings")]
    public bool isDraggable = true;

    [Header("Weight")]
    [Tooltip("Object weight in kg. Affects drag force, player speed and object damping.")]
    public float weight = 5f;

    [Header("Events")]
    public UnityEvent onInteract;
    public UnityEvent onPickUp;

    private Rigidbody _rb;

    void Awake() => _rb = GetComponent<Rigidbody>();

    // Called by PlayerInteraction when dragging starts
    public void StartDrag(float damping)
    {
        if (_rb) _rb.linearDamping = damping;
    }

    // Called every frame while being dragged
    public void DragTowards(Vector3 targetPos, float force)
    {
        if (!isDraggable || _rb == null) return;
        Vector3 direction = targetPos - transform.position;
        _rb.AddForce(direction * force, ForceMode.Force);
    }

    public void StopDrag() { if (_rb) _rb.linearDamping = 1f; }

    // Rotates the object around camera axes using mouse delta
    public void ApplyRotation(Vector2 mouseDelta, Transform cameraTransform, float sensitivity)
    {
        if (_rb == null) return;
        _rb.MoveRotation(_rb.rotation
            * Quaternion.AngleAxis(mouseDelta.x * sensitivity, cameraTransform.up)
            * Quaternion.AngleAxis(-mouseDelta.y * sensitivity, cameraTransform.right));
    }

    public void Interact() => onInteract?.Invoke();
    public void PickUp() => onPickUp?.Invoke();
}
