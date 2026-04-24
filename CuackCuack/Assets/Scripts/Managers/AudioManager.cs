using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized audio manager. Handles background music with crossfade and
/// sound effects via an AudioSource pool. Singleton — DontDestroyOnLoad.
///
/// Usage:
///   AudioManager.Instance.PlaySFX("footstep");
///   AudioManager.Instance.PlayMusic("theme_01");
///   AudioManager.Instance.SetMasterVolume(0.8f);
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Audio Library")]
    [Tooltip("All sound effect clips. Name them here to call PlaySFX(name).")]
    public SoundEntry[] sounds;

    [Tooltip("All music tracks. Name them here to call PlayMusic(name).")]
    public SoundEntry[] musicTracks;

    [Header("SFX Pool")]
    [Tooltip("Number of AudioSources in the pool. Increase if sounds cut off.")]
    public int poolSize = 8;

    [Header("Music")]
    public float crossfadeDuration = 1.5f;

    [Header("Default Volumes")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    // ── Internal ──────────────────────────────────────────────────────────────

    private readonly Dictionary<string, SoundEntry> _sfxMap = new();
    private readonly Dictionary<string, SoundEntry> _musicMap = new();

    private List<AudioSource> _sfxPool;
    private AudioSource _musicSourceA;
    private AudioSource _musicSourceB;
    private bool _musicOnA = true;
    private Coroutine _crossfadeCoroutine;

    // ── Singleton ─────────────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildMaps();
        BuildPool();
        BuildMusicSources();
    }

    void OnEnable() => GameManager.OnPauseChanged += OnPauseChanged;
    void OnDisable() => GameManager.OnPauseChanged -= OnPauseChanged;

    // ── Initialisation ────────────────────────────────────────────────────────

    void BuildMaps()
    {
        foreach (var s in sounds) _sfxMap[s.name] = s;
        foreach (var m in musicTracks) _musicMap[m.name] = m;
    }

    void BuildPool()
    {
        _sfxPool = new List<AudioSource>(poolSize);
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"SFX_Pool_{i}");
            go.transform.parent = transform;
            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            _sfxPool.Add(src);
        }
    }

    void BuildMusicSources()
    {
        _musicSourceA = CreateMusicSource("Music_A");
        _musicSourceB = CreateMusicSource("Music_B");
    }

    AudioSource CreateMusicSource(string goName)
    {
        var go = new GameObject(goName);
        go.transform.parent = transform;
        var src = go.AddComponent<AudioSource>();
        src.playOnAwake = false;
        src.loop = true;
        src.volume = 0f;
        return src;
    }

    // ── SFX API ───────────────────────────────────────────────────────────────

    /// <summary>Plays a sound effect by name (defined in the Inspector).</summary>
    public void PlaySFX(string soundName)
    {
        if (!_sfxMap.TryGetValue(soundName, out var entry))
        {
            Debug.LogWarning($"[AudioManager] SFX '{soundName}' not found.");
            return;
        }
        var src = GetFreeSource();
        src.clip = entry.clip;
        src.volume = entry.volume * sfxVolume * masterVolume;
        src.pitch = entry.randomPitch
            ? Random.Range(entry.pitchMin, entry.pitchMax)
            : 1f;
        src.Play();
    }

    /// <summary>Plays a sound effect at a world position (3D).</summary>
    public void PlaySFXAt(string soundName, Vector3 position)
    {
        if (!_sfxMap.TryGetValue(soundName, out var entry)) return;
        AudioSource.PlayClipAtPoint(entry.clip, position, entry.volume * sfxVolume * masterVolume);
    }

    AudioSource GetFreeSource()
    {
        foreach (var src in _sfxPool)
            if (!src.isPlaying) return src;

        // All busy — reuse the first (oldest) one
        return _sfxPool[0];
    }

    // ── Music API ─────────────────────────────────────────────────────────────

    /// <summary>Crossfades to a new music track.</summary>
    public void PlayMusic(string trackName)
    {
        if (!_musicMap.TryGetValue(trackName, out var entry))
        {
            Debug.LogWarning($"[AudioManager] Music '{trackName}' not found.");
            return;
        }

        if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
        _crossfadeCoroutine = StartCoroutine(Crossfade(entry));
    }

    public void StopMusic()
    {
        if (_crossfadeCoroutine != null) StopCoroutine(_crossfadeCoroutine);
        _musicSourceA.Stop();
        _musicSourceB.Stop();
    }

    System.Collections.IEnumerator Crossfade(SoundEntry entry)
    {
        AudioSource incoming = _musicOnA ? _musicSourceB : _musicSourceA;
        AudioSource outgoing = _musicOnA ? _musicSourceA : _musicSourceB;

        float targetVol = entry.volume * musicVolume * masterVolume;
        incoming.clip = entry.clip;
        incoming.Play();

        float t = 0f;
        while (t < crossfadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float ratio = t / crossfadeDuration;
            incoming.volume = Mathf.Lerp(0f, targetVol, ratio);
            outgoing.volume = Mathf.Lerp(outgoing.volume, 0f, ratio);
            yield return null;
        }

        outgoing.Stop();
        outgoing.volume = 0f;
        _musicOnA = !_musicOnA;
    }

    // ── Volume control ────────────────────────────────────────────────────────

    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        RefreshMusicVolume();
    }

    public void SetSFXVolume(float v) => sfxVolume = Mathf.Clamp01(v);

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        RefreshMusicVolume();
    }

    void RefreshMusicVolume()
    {
        AudioSource active = _musicOnA ? _musicSourceA : _musicSourceB;
        if (active.isPlaying) active.volume = musicVolume * masterVolume;
    }

    // ── Pause integration ─────────────────────────────────────────────────────

    void OnPauseChanged(bool paused)
    {
        AudioSource active = _musicOnA ? _musicSourceA : _musicSourceB;
        if (paused) active.Pause();
        else active.UnPause();
    }
}

// ── Data types ────────────────────────────────────────────────────────────────

[System.Serializable]
public class SoundEntry
{
    public string name;
    public AudioClip clip;
    [Range(0f, 1f)] public float volume = 1f;
    public bool randomPitch;
    [Range(0.5f, 2f)] public float pitchMin = 0.9f;
    [Range(0.5f, 2f)] public float pitchMax = 1.1f;
}