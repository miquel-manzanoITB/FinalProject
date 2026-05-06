using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Gestiona los paneles de UI: HUD, pausa y pantalla de carga.
/// Singleton por escena — activa DontDestroyOnLoad si necesitas persistencia entre escenas.
/// Se suscribe a los eventos de GameManager para reaccionar a cambios de estado.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject loadingPanel;

    [Header("HUD Elements")]
    public TextMeshProUGUI interactionHintText; // "Press E to interact"

    // ── Singleton ─────────────────────────────────────────────────────────────

    void Awake()
    {
        // Si ya existe una instancia, destruimos el duplicado y salimos
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnDestroy()
    {
        // Limpiamos la referencia estática para evitar referencias huérfanas
        if (Instance == this) Instance = null;
    }

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void OnEnable()
    {
        // Nos suscribimos a los eventos del GameManager mientras el objeto esté activo
        GameManager.OnPauseChanged += OnPauseChanged;
        GameManager.OnGameStart    += OnGameStart;
    }

    void OnDisable()
    {
        // Siempre desuscribirse para evitar llamadas a objetos destruidos o inactivos
        GameManager.OnPauseChanged -= OnPauseChanged;
        GameManager.OnGameStart    -= OnGameStart;
    }

    void Start()
    {
        // Estado inicial: solo el HUD visible. Evita que el Inspector dicte el estado de arranque
        SetPanel(hudPanel,     true);
        SetPanel(pausePanel,   false);
        SetPanel(loadingPanel, false);
    }

    // ── Panel control ─────────────────────────────────────────────────────────

    // Restaura el estado de juego activo al iniciar o reiniciar la partida
    void OnGameStart()
    {
        SetPanel(hudPanel,   true);
        SetPanel(pausePanel, false);
    }

    // Alterna entre HUD y menú de pausa según el estado recibido
    void OnPauseChanged(bool paused)
    {
        SetPanel(pausePanel, paused);
        SetPanel(hudPanel,   !paused);
    }

    /// <summary>Muestra u oculta la pantalla de carga. Llamar desde GameManager al cambiar de escena.</summary>
    public void ShowLoadingScreen(bool show) => SetPanel(loadingPanel, show);

    // Activa o desactiva un panel de forma segura (null-safe)
    static void SetPanel(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    // ── HUD helpers ───────────────────────────────────────────────────────────

    /// <summary>
    /// Muestra u oculta el hint de interacción según el mensaje recibido.
    /// Llamar desde PlayerInteraction con el texto deseado, o vacío para ocultar.
    /// </summary>
    public void ShowInteractionHint(string message)
    {
        if (interactionHintText == null) return;
        interactionHintText.text = message;
        interactionHintText.gameObject.SetActive(!string.IsNullOrEmpty(message));
    }

    /// <summary>Oculta el hint de interacción.</summary>
    public void HideInteractionHint() => ShowInteractionHint(string.Empty);
}