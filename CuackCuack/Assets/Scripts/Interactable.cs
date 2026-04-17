using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Settings")]
    public bool isDraggable = true;       // ¿Se puede arrastrar?

    [Header("Drag Physics")]
    public float dragForce = 50f;
    public float maxDragDistance = 3f;     // Distancia máxima al arrastrar

    [Header("Events")]
    public UnityEvent onInteract;          // Para conectar lógica custom desde el Inspector
    public UnityEvent onPickUp;

    private Rigidbody _rb;

    void Awake() => _rb = GetComponent<Rigidbody>();

    // Llamado por PlayerInteraction cuando empieza el arrastre
    public void StartDrag() { if (_rb) _rb.linearDamping = 10f; }

    // Llamado cada frame mientras se arrastra
    public void DragTowards(Vector3 targetPos)
    {
        if (!isDraggable || _rb == null) return;
        Vector3 dir = targetPos - transform.position;
        _rb.AddForce(dir * dragForce, ForceMode.Force);
    }

    public void ApplyRotation(Vector2 mouseDelta, Transform cameraTransform, float sensitivity)
    {
        if (_rb == null) return;
        _rb.MoveRotation(_rb.rotation
            * Quaternion.AngleAxis(mouseDelta.x * sensitivity, cameraTransform.up)
            * Quaternion.AngleAxis(-mouseDelta.y * sensitivity, cameraTransform.right));
    }

    public void StopDrag() { if (_rb) _rb.linearDamping = 1f; }

    public void Interact() => onInteract?.Invoke();
    public void PickUp() => onPickUp?.Invoke();
}