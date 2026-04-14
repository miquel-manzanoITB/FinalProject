using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Raycast")]
    public Camera playerCamera;
    public float interactRange = 3f;
    public LayerMask interactableLayer;

    [Header("Drag")]
    public float scrollSpeed = 0.5f;
    public float minDragDistance = 1f;
    public float maxDragDistance = 4f;

    private Interactable _hovered;
    private Interactable _dragging;
    private float _dragDistance;

    void Update()
    {
        HandleHover();
        HandleDragInput();
        HandleScroll();
        HandleInteract();
    }

    void HandleInteract()
    {
        // Al pulsar E cerca de un objeto → Interact()
        if (Keyboard.current.eKey.wasPressedThisFrame && _hovered != null)
            _hovered.Interact();
    }

    // Detecta qué objeto está mirando el jugador
    void HandleHover()
    {
        Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
        {
            var interactable = hit.collider.GetComponent<Interactable>();
            if (interactable != _hovered)
            {
                _hovered = interactable;
                // Aquí podrías activar un crosshair o hint en el futuro
            }
        }
        else
        {
            _hovered = null;
        }
    }

    void HandleDragInput()
    {
        // Empezar arrastre
        if (Mouse.current.leftButton.wasPressedThisFrame && _hovered != null && _dragging == null)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableLayer))
            {
                _dragging = _hovered;
                _dragDistance = hit.distance;
                _dragging.StartDrag();
            }
        }

        // Arrastrar cada frame
        if (_dragging != null && Mouse.current.leftButton.isPressed)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            Vector3 targetPos = ray.GetPoint(_dragDistance);
            _dragging.DragTowards(targetPos);
        }

        // Soltar
        if (Mouse.current.leftButton.wasReleasedThisFrame && _dragging != null)
        {
            _dragging.StopDrag();
            _dragging = null;
        }
    }

    // Rueda del ratón: acercar o alejar el objeto mientras se arrastra
    void HandleScroll()
    {
        if (_dragging == null) return;
        float scroll = Mouse.current.scroll.ReadValue().y;
        _dragDistance += scroll * scrollSpeed * Time.deltaTime;
        _dragDistance = Mathf.Clamp(_dragDistance, minDragDistance, maxDragDistance);
    }
}