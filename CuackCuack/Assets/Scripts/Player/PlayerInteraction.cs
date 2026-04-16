using UnityEngine;
using UnityEngine.UI;

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

    [Header("Crosshair")]
    public Image crosshair;

    private Interactable _hovered;
    private Interactable _dragging;
    private float _dragDistance;
    private bool _isDragging;
    private float _scrollDirection;

    private PlayerInputController _input;

    void Awake()
    {
        _input = GetComponent<PlayerInputController>();
    }

    void OnEnable()
    {
        _input.OnInteractEvent += OnInteract;
        _input.OnPickUpEvent += OnPickUp;
        _input.OnDropEvent += OnDrop;
        _input.OnScrollEvent += OnScroll;
    }

    void OnDisable()
    {
        _input.OnInteractEvent -= OnInteract;
        _input.OnPickUpEvent -= OnPickUp;
        _input.OnDropEvent -= OnDrop;
        _input.OnScrollEvent -= OnScroll;
    }

    void Update()
    {
        HandleHover();
        HandleDragInput();
        HandleScroll();
    }

    void OnInteract()
    {
        if (_hovered != null)
            _hovered.Interact();
    }

    void OnPickUp()
    {
        _isDragging = true;
    }
    void OnDrop()
    {
        _isDragging = false;
    }

    void OnScroll(Vector2 dir)
    {
        _scrollDirection = dir.y;
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
                crosshair.color = Color.green; // Ejemplo: cambiar color del crosshair
            }
        }
        else
        {
            _hovered = null;
            crosshair.color = Color.white; // Ejemplo: cambiar color del crosshair cuando no hay objeto
        }
        Debug.DrawRay(ray.origin, ray.direction * interactRange, _hovered != null ? Color.green : Color.red);
    }

    void HandleDragInput()
    {
        // Empezar arrastre
        if (_isDragging && _hovered != null && _dragging == null)
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
        if (_dragging != null && _isDragging)
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            Vector3 targetPos = ray.GetPoint(_dragDistance);
            _dragging.DragTowards(targetPos);
        }

        // Soltar
        if (!_isDragging && _dragging != null)
        {
            _dragging.StopDrag();
            _dragging = null;
        }
    }

    // Rueda del ratón: acercar o alejar el objeto mientras se arrastra
    void HandleScroll()
    {
        if (_dragging == null) return;
        _dragDistance += _scrollDirection * scrollSpeed * Time.deltaTime;
        _dragDistance = Mathf.Clamp(_dragDistance, minDragDistance, maxDragDistance);
    }
}