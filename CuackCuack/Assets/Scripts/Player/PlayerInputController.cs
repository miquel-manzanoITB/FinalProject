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
    public event UnityAction OnJumpEvent = delegate { };
    public event UnityAction OnInteractEvent = delegate { };

    public static event UnityAction OnPauseEvent;   // static so the UI can listen without a reference

    // ── Internal ──────────────────────────────────────────────────────────────

    private InputSystem_Actions _inputActions;

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void Awake()
    {
        _inputActions = new InputSystem_Actions();
        _inputActions.Player.SetCallbacks(this);
    }

    void OnEnable() => _inputActions.Enable();
    void OnDisable() => _inputActions.Disable();

    // ── IPlayerActions callbacks ──────────────────────────────────────────────

    public void OnMove(InputAction.CallbackContext context)
        => OnMoveEvent.Invoke(context.ReadValue<Vector2>());

    public void OnLook(InputAction.CallbackContext context)
        => OnLookEvent.Invoke(context.ReadValue<Vector2>());

    public void OnJump(InputAction.CallbackContext context)
    {
        OnJumpEvent.Invoke();
        // No va el context.performed
        if (context.performed) {
            Debug.Log("Jump input received 22");
            OnJumpEvent.Invoke();
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        OnInteractEvent.Invoke();
        Debug.Log("Interact input received fuera context");
        if (context.performed)
        {
            Debug.Log("performed");
            OnInteractEvent.Invoke();
        }
        // No va el context.performed
        if (context.started)
        {
            Debug.Log("started");
            OnInteractEvent.Invoke();
        }
        if (context.canceled)
        {
            Debug.Log("canceled");
            OnInteractEvent.Invoke();
        }
    }

    public void OnPauseGame(InputAction.CallbackContext context)
    {
        Debug.Log("Pause input received");
        if (context.performed) OnPauseEvent?.Invoke();
    }

    public void OnPickUp(InputAction.CallbackContext context)
    {
        Debug.Log("PickUp input received");
        throw new System.NotImplementedException();
    }
}