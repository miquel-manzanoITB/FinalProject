using UnityEngine;

/// <summary>
/// Handles player movement: walking, jumping and drag.
/// Attach to the Player root GameObject.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Walk")]
    public float moveSpeed = 4f;

    [Header("Jump")]
    public float jumpForce = 5f;

    [Header("Drag")]
    public float groundDrag = 6f;
    public float airDrag = 1f;

    [Header("Ground Check")]
    public float rayLength = 1.1f;
    public LayerMask groundLayer;
    public float groundCheckRadius;

    // ── Internal ──────────────────────────────────────────────────────────────

    private Rigidbody _rb;
    private PlayerInputController _input;
    private PlayerCamera _playerCamera;
    private Vector2 _moveInput;
    private bool _isGrounded;
    private float _speedPenalty = 1f;   // 1 = no penalty, <1 = slower

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _input = GetComponent<PlayerInputController>();
        _playerCamera = GetComponent<PlayerCamera>();

        _rb.freezeRotation = true;
    }

    void OnEnable()
    {
        _input.OnMoveEvent += OnMove;
        _input.OnJumpEvent += OnJump;
    }

    void OnDisable()
    {
        _input.OnMoveEvent -= OnMove;
        _input.OnJumpEvent -= OnJump;
    }

    void Update()
    {
        CheckGround();
        ApplyDrag();
        _playerCamera.SetMoving(_moveInput != Vector2.zero && _isGrounded);
    }

    void FixedUpdate() { Move(); }

    // ── Public API (used by PlayerInteraction for weight penalty) ─────────────

    /// <param name="penalty">Speed multiplier: 1 = full speed, 0.3 = 30% speed.</param>
    public void SetSpeedPenalty(float penalty) => _speedPenalty = Mathf.Clamp01(penalty);

    public void ResetSpeedPenalty() => _speedPenalty = 1f;

    // ── Input handlers ────────────────────────────────────────────────────────

    void OnMove(Vector2 input) => _moveInput = input;

    void OnJump()
    {
        if (_isGrounded) Jump();
    }

    // ── Private methods ───────────────────────────────────────────────────────

    void Move()
    {
        Vector3 direction = transform.right * _moveInput.x
                          + transform.forward * _moveInput.y;

        float effectiveSpeed = moveSpeed * _speedPenalty;
        _rb.AddForce(direction * effectiveSpeed, ForceMode.VelocityChange);

        // Cap horizontal speed
        Vector3 flatVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        if (flatVelocity.magnitude > effectiveSpeed)
        {
            Vector3 capped = flatVelocity.normalized * effectiveSpeed;
            _rb.linearVelocity = new Vector3(capped.x, _rb.linearVelocity.y, capped.z);
        }
    }

    void Jump()
    {
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    public void SuperJump()
    {
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _rb.AddForce(Vector3.up * 100, ForceMode.Impulse);
    }

    void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * (groundCheckRadius + 0.1f);
        _isGrounded = Physics.Raycast(origin, Vector3.down, rayLength, groundLayer);
        Debug.DrawRay(origin, Vector3.down * rayLength, _isGrounded ? Color.green : Color.red);
    }

    void ApplyDrag()
    {
        _rb.linearDamping = _isGrounded ? groundDrag : airDrag;
    }
}
