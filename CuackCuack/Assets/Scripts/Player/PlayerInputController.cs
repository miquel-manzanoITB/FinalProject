using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static InputSystem_Actions;

/// <summary>
/// Single entry point for all player input.
/// Translates raw input into events that other scripts subscribe to.
/// Attach to the Player root GameObject.
/// </summary>
public class PlayerInputController : MonoBehaviour, IPlayerActions
{
    // ── Events ────────────────────────────────────────────────────────────────

    public event UnityAction<Vector2> OnMoveEvent = delegate { };
    public event UnityAction<Vector2> OnLookEvent = delegate { };
    public event UnityAction<Vector2> OnScrollEvent = delegate { };
    public event UnityAction OnJumpEvent = delegate { };
    public event UnityAction OnInteractEvent = delegate { };
    public event UnityAction OnPickUpEvent = delegate { };
    public event UnityAction OnDropEvent = delegate { };

    /// <summary>Fires when R is held — carries raw mouse delta for object rotation.</summary>
    public event UnityAction<Vector2> OnRotateObjectEvent = delegate { };

    public static event UnityAction OnPauseEvent;

    // ── Internal ──────────────────────────────────────────────────────────────

    private InputSystem_Actions _inputActions;
    private bool _isRotating;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Player.SetCallbacks(this);
    }

    void OnEnable() => _inputActions.Enable();
    void OnDisable() => _inputActions.Disable();

    void Update()
    {
        // While R is held, forward the current mouse delta every frame
        if (_isRotating)
            OnRotateObjectEvent.Invoke(_inputActions.Player.Look.ReadValue<Vector2>());
    }

    // ── IPlayerActions callbacks ──────────────────────────────────────────────

    public void OnMove(InputAction.CallbackContext context)
        => OnMoveEvent.Invoke(context.ReadValue<Vector2>());

    public void OnLook(InputAction.CallbackContext context)
        => OnLookEvent.Invoke(context.ReadValue<Vector2>());

    public void OnScroll(InputAction.CallbackContext context)
        => OnScrollEvent.Invoke(context.ReadValue<Vector2>());

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.started) OnJumpEvent.Invoke();
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started) OnInteractEvent.Invoke();
    }

    public void OnPauseGame(InputAction.CallbackContext context)
    {
        if (context.performed) OnPauseEvent?.Invoke();
    }

    public void OnPickUp(InputAction.CallbackContext context)
    {
        if (context.started) OnPickUpEvent.Invoke();
        if (context.canceled) OnDropEvent.Invoke();
    }

    /// <summary>Bound to the RotateObject action (R key) in the Input Asset.</summary>
    public void OnRotateObject(InputAction.CallbackContext context)
    {
        if (context.started) _isRotating = true;
        if (context.canceled) _isRotating = false;
    }
}
