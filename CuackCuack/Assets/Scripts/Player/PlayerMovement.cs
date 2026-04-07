using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 4f;
    public float groundDrag = 6f;
    public float airDrag = 1f;
    public float jumpForce = 5f;

    [Header("Ground Check")]
    public float rayLength = 1.1f;
    public LayerMask groundLayer;

    [Header("Look")]
    public float mouseSensitivity = 0.2f;
    public Transform cameraTransform;

    private Rigidbody _rb;
    private Vector2 _moveInput;
    private Vector2 _lookInput;
    private float _cameraPitch;
    private bool _isGrounded;

    public void OnMove(InputValue value) => _moveInput = value.Get<Vector2>();
    public void OnLook(InputValue value) => _lookInput = value.Get<Vector2>();
    public void OnJump(InputValue value) { if (value.isPressed && _isGrounded) Jump(); }

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _rb.freezeRotation = true;
    }

    void Update()
    {
        HandleLook();
        CheckGround();
        ApplyDrag();
    }

    void FixedUpdate()
    {
        HandleMove();
    }

    void HandleLook()
    {
        transform.Rotate(Vector3.up * _lookInput.x * mouseSensitivity);
        _cameraPitch -= _lookInput.y * mouseSensitivity;
        _cameraPitch = Mathf.Clamp(_cameraPitch, -80f, 80f);
        cameraTransform.localRotation = Quaternion.Euler(_cameraPitch, 0f, 0f);
    }

    void HandleMove()
    {
        Vector3 direction = transform.right * _moveInput.x + transform.forward * _moveInput.y;
        _rb.AddForce(direction * moveSpeed, ForceMode.VelocityChange);

        Vector3 flatVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        if (flatVelocity.magnitude > moveSpeed)
        {
            Vector3 capped = flatVelocity.normalized * moveSpeed;
            _rb.linearVelocity = new Vector3(capped.x, _rb.linearVelocity.y, capped.z);
        }
    }

    void Jump()
    {
        _rb.linearVelocity = new Vector3(_rb.linearVelocity.x, 0f, _rb.linearVelocity.z);
        _rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    void CheckGround()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        _isGrounded = Physics.Raycast(ray, rayLength, groundLayer);
    }

    void ApplyDrag()
    {
        _rb.linearDamping = _isGrounded ? groundDrag : airDrag;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = _isGrounded ? Color.green : Color.red;
        Vector3 start = transform.position;
        Vector3 end = transform.position + Vector3.down * rayLength;
        Gizmos.DrawLine(start, end);
        Gizmos.DrawWireSphere(end, 0.05f);
    }
}