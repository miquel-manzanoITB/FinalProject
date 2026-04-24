using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages all UI panels: HUD, pause menu, game over screen, loading screen.
/// Singleton — add one instance to each scene that needs UI, or keep it DontDestroyOnLoad.
/// Pair with GameManager events.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panels")]
    public GameObject hudPanel;
    public GameObject pausePanel;
    public GameObject gameOverPanel;
    public GameObject loadingPanel;
    public GameObject mainMenuPanel;

    [Header("HUD Elements")]
    public TextMeshProUGUI interactionHintText;   // "Press E to interact"

    [Header("Pause Menu Buttons")]
    public Button resumeButton;
    public Button restartButton;
    public Button mainMenuButton;
    public Button quitButton;

    [Header("Game Over")]
    public Button gameOverRestartButton;
    public Button gameOverMainMenuButton;
    public TextMeshProUGUI gameOverTitleText;

    // ── Singleton ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ── Unity lifecycle ───────────────────────────────────────────────────────

    void OnEnable()
    {
        GameManager.OnPauseChanged += OnPauseChanged;
        GameManager.OnGameOver += OnGameOver;
        GameManager.OnGameStart += OnGameStart;

        resumeButton?.onClick.AddListener(() => GameManager.Instance.TogglePause());
        restartButton?.onClick.AddListener(() => GameManager.Instance.ReloadCurrentScene());
        mainMenuButton?.onClick.AddListener(() => GameManager.Instance.LoadMainMenu());
        quitButton?.onClick.AddListener(() => GameManager.Instance.QuitGame());

        gameOverRestartButton?.onClick.AddListener(() => GameManager.Instance.ReloadCurrentScene());
        gameOverMainMenuButton?.onClick.AddListener(() => GameManager.Instance.LoadMainMenu());
    }

    void OnDisable()
    {
        GameManager.OnPauseChanged -= OnPauseChanged;
        GameManager.OnGameOver -= OnGameOver;
        GameManager.OnGameStart -= OnGameStart;
    }

    // ── Panel control ─────────────────────────────────────────────────────────

    void OnGameStart()
    {
        SetPanel(hudPanel, true);
        SetPanel(pausePanel, false);
        SetPanel(gameOverPanel, false);
        SetPanel(mainMenuPanel, false);
    }

    void OnPauseChanged(bool paused)
    {
        SetPanel(pausePanel, paused);
        SetPanel(hudPanel, !paused && !GameManager.Instance.IsGameOver);
    }

    void OnGameOver()
    {
        SetPanel(gameOverPanel, true);
        SetPanel(hudPanel, false);
    }

    public void ShowLoadingScreen(bool show) => SetPanel(loadingPanel, show);

    static void SetPanel(GameObject panel, bool active)
    {
        if (panel != null) panel.SetActive(active);
    }

    // ── HUD helpers ───────────────────────────────────────────────────────────

    /// <summary>Call from PlayerInteraction to show/hide the "Press E" hint.</summary>
    public void ShowInteractionHint(string message)
    {
        if (interactionHintText == null) return;
        interactionHintText.text = message;
        interactionHintText.gameObject.SetActive(!string.IsNullOrEmpty(message));
    }

    public void HideInteractionHint() => ShowInteractionHint(string.Empty);
}