using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages hover detection, object dragging, distance scrolling and object rotation.
/// Weight-based behaviour is driven by the shared WeightConfig asset.
/// </summary>
public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    public Camera playerCamera;
    public float interactRange = 3f;
    public LayerMask interactableLayer;

    [Header("Drag Distance")]
    public float scrollSpeed = 5f;          // units/sec
    public float minDragDistance = 1f;
    public float maxDragDistance = 4f;

    [Header("Object Rotation")]
    [Tooltip("Mouse sensitivity when rotating a held object with R + mouse.")]
    public float rotateSensitivity = 0.2f;

    [Header("Weight")]
    public WeightConfig weightConfig;

    [Header("Crosshair")]
    public Image crosshair;

    // ── State ─────────────────────────────────────────────────────────────────

    private Interactable _hovered;
    private Interactable _dragging;
    private float _dragDistance;
    private bool _isDragging;
    private float _scrollDelta;       // raw scroll this frame
    private bool _isRotating;
    private Vector2 _rotateDelta;

    private PlayerInputController _input;
    private PlayerMovement _movement;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _input = GetComponent<PlayerInputController>();
        _movement = GetComponent<PlayerMovement>();
    }

    void OnEnable()
    {
        _input.OnInteractEvent += OnInteract;
        _input.OnPickUpEvent += OnPickUp;
        _input.OnDropEvent += OnDrop;
        _input.OnScrollEvent += OnScroll;
        _input.OnRotateObjectEvent += OnRotateObject;
    }

    void OnDisable()
    {
        _input.OnInteractEvent -= OnInteract;
        _input.OnPickUpEvent -= OnPickUp;
        _input.OnDropEvent -= OnDrop;
        _input.OnScrollEvent -= OnScroll;
        _input.OnRotateObjectEvent -= OnRotateObject;
    }

    void Update()
    {
        HandleHover();
        HandleDragInput();
        HandleScroll();
        HandleRotation();

        // Consume scroll delta each frame — prevents accumulation
        _scrollDelta = 0f;
    }

    // ── Input handlers ────────────────────────────────────────────────────────

    void OnInteract() { _hovered?.Interact(); }

    void OnPickUp()
    {
        _isDragging = true;
        _hovered?.PickUp();
    }

    void OnDrop() { _isDragging = false; }

    void OnScroll(Vector2 dir) { _scrollDelta = dir.y; }

    void OnRotateObject(Vector2 delta)
    {
        if (_dragging != null) _rotateDelta = delta;
    }

    // ── Private methods ───────────────────────────────────────────────────────

    void HandleHover()
    {
        Ray ray = CenterRay();

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            var interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != _hovered)
                _hovered = interactable;

            crosshair.color = Color.green;
        }
        else
        {
            _hovered = null;
            crosshair.color = Color.white;
        }

        Debug.DrawRay(ray.origin, ray.direction * interactRange,
                      _hovered != null ? Color.green : Color.red);
    }

    void HandleDragInput()
    {
        // ── Start dragging ────────────────────────────────────────────────────
        if (_isDragging && _hovered != null && _dragging == null)
        {
            Ray ray = CenterRay();
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
            {
                _dragging = _hovered;
                _dragDistance = hit.distance;

                // Give the object a reference to the config so it can self-manage gravity/collision
                if (weightConfig != null)
                    _dragging.InjectWeightConfig(weightConfig);

                float damping = weightConfig
                    ? weightConfig.GetDragDamping(_dragging.weight)
                    : 10f;

                _dragging.StartDrag(damping);
                ApplyWeightSpeedPenalty(_dragging.weight);
            }
        }

        // ── Drag every frame ──────────────────────────────────────────────────
        if (_dragging != null && _isDragging)
        {
            Vector3 targetPos = CenterRay().GetPoint(_dragDistance);
            float force = weightConfig
                ? weightConfig.GetDragForce(_dragging.weight)
                : 50f;

            _dragging.DragTowards(targetPos, force);
        }

        // ── Release ───────────────────────────────────────────────────────────
        if (!_isDragging && _dragging != null)
        {
            _dragging.StopDrag();
            _dragging = null;
            _movement.ResetSpeedPenalty();
        }
    }

    void HandleScroll()
    {
        if (_dragging == null) return;

        // ── FIX: scroll only moves the drag distance — never pushes the player ──
        // We clamp before applying so the object never tries to move behind the
        // camera or beyond reach, which was what caused the object to push the player.
        float newDistance = _dragDistance + _scrollDelta * scrollSpeed * Time.deltaTime;
        _dragDistance = Mathf.Clamp(newDistance, minDragDistance, maxDragDistance);
    }

    void HandleRotation()
    {
        if (_dragging == null || _rotateDelta == Vector2.zero) return;
        _dragging.ApplyRotation(_rotateDelta, playerCamera.transform, rotateSensitivity);
        _rotateDelta = Vector2.zero;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    Ray CenterRay() =>
        playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2f, Screen.height / 2f));

    void ApplyWeightSpeedPenalty(float weight)
    {
        if (weightConfig == null) return;
        float penalty = weightConfig.GetSpeedPenalty(weight);
        _movement.SetSpeedPenalty(penalty);
    }
}
