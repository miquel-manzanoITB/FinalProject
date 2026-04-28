using System.IO;
using UnityEngine;

/// <summary>
/// Lightweight save/load system.
/// Serialises a SaveData object to JSON in Application.persistentDataPath.
/// Also wraps PlayerPrefs for quick settings (volume, sensitivity, etc.).
///
/// Usage:
///   SaveManager.Instance.Save();
///   SaveData data = SaveManager.Instance.Load();
///   SaveManager.Instance.SetSetting("masterVolume", 0.8f);
///   float vol = SaveManager.Instance.GetSetting("masterVolume", 1f);
/// </summary>
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private const string SAVE_FILE = "save.json";

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── Save / Load ───────────────────────────────────────────────────────────

    string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE);

    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, prettyPrint: true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveManager] Saved to {SavePath}");
    }

    /// <summary>Returns null if no save file exists.</summary>
    public SaveData Load()
    {
        if (!File.Exists(SavePath)) return null;
        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public bool HasSave() => File.Exists(SavePath);

    public void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
    }

    // ── Settings (PlayerPrefs) ────────────────────────────────────────────────

    public void SetSetting(string key, float value) => PlayerPrefs.SetFloat(key, value);
    public void SetSetting(string key, int value) => PlayerPrefs.SetInt(key, value);
    public void SetSetting(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);

    public float GetSetting(string key, float defaultValue = 0f) => PlayerPrefs.GetFloat(key, defaultValue);
    public int GetSetting(string key, int defaultValue = 0) => PlayerPrefs.GetInt(key, defaultValue);
    public bool GetSetting(string key, bool defaultValue = false) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;

    public void ApplySavedSettings()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMasterVolume(GetSetting("masterVolume", 1f));
            AudioManager.Instance.SetSFXVolume(GetSetting("sfxVolume", 1f));
            AudioManager.Instance.SetMusicVolume(GetSetting("musicVolume", 0.5f));
        }

        if (FindFirstObjectByType<PlayerCamera>() is PlayerCamera cam)
        {
            cam.SetSensitivity(
                GetSetting("sensitivityX", 0.2f),
                GetSetting("sensitivityY", 0.2f));
            cam.SetBobEnabled(GetSetting("bobEnabled", true));
        }
    }
}

// ── Save data model — extend this with your game's fields ─────────────────────

[System.Serializable]
public class SaveData
{
    public string lastScene;
    public float playTimeSeconds;

    // Example gameplay fields — replace / extend as needed:
    public int[] collectedItems;
    public bool[] completedLevels;
}