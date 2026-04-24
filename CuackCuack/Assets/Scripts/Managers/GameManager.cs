using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;

/// <summary>
/// Central game manager. Handles scene transitions, pause state and global game flow.
/// Singleton — persists across scenes. Place on a single GameObject in your first scene.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // ── Events ────────────────────────────────────────────────────────────────

    public static event UnityAction<bool> OnPauseChanged;   // true = paused
    public static event UnityAction OnGameStart;
    public static event UnityAction OnGameOver;

    // ── State ─────────────────────────────────────────────────────────────────

    public bool IsPaused { get; private set; }
    public bool IsGameOver { get; private set; }

    [Header("Scene Names")]
    public string mainMenuScene = "MainMenu";
    public string firstGameScene = "Level01";

    // ── Singleton setup ───────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        PlayerInputController.OnPauseEvent += TogglePause;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        PlayerInputController.OnPauseEvent -= TogglePause;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ── Scene management ──────────────────────────────────────────────────────

    public void LoadScene(string sceneName)
    {
        SetPause(false);            // always unpause before transitioning
        UIManager.Instance?.ShowLoadingScreen(true);
        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int buildIndex) => LoadScene(SceneManager.GetSceneByBuildIndex(buildIndex).name);

    public void ReloadCurrentScene() => LoadScene(SceneManager.GetActiveScene().name);

    public void LoadMainMenu() => LoadScene(mainMenuScene);

    public void StartGame() => LoadScene(firstGameScene);

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UIManager.Instance?.ShowLoadingScreen(false);
        IsGameOver = false;

        bool isGameScene = scene.name != mainMenuScene;
        if (isGameScene) OnGameStart?.Invoke();
    }

    // ── Pause ─────────────────────────────────────────────────────────────────

    public void TogglePause() => SetPause(!IsPaused);

    public void SetPause(bool paused)
    {
        if (IsPaused == paused) return;

        IsPaused = paused;
        Time.timeScale = paused ? 0f : 1f;

        Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = paused;

        OnPauseChanged?.Invoke(paused);
    }

    // ── Game over ─────────────────────────────────────────────────────────────

    public void TriggerGameOver()
    {
        if (IsGameOver) return;
        IsGameOver = true;
        SetPause(true);
        OnGameOver?.Invoke();
    }
}