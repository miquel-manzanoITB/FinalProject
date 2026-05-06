using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Persist Between Scenes")]
    public bool dontDestroyOnLoad = true;

    [Header("Volume")]
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.5f;

    [Header("SFX Prefab")]
    [Tooltip("Prefab con AudioSource configurado. Si es null se crea uno básico.")]
    public AudioSource soundFXObject;

    [Header("Sound Effects")]
    public SoundEffect[] soundEffects;

    [Header("Background Music")]
    [Tooltip("Lista de pistas de música. El índice 0 se reproduce al arrancar.")]
    public AudioClip[] backgroundMusics;

    [Tooltip("Si está activado, al llamar NextMusic() vuelve al índice 0 al llegar al final.")]
    public bool loopPlaylist = true;

    [System.Serializable]
    public class SoundEffect
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(0.5f, 2f)] public float pitch = 1f;
    }

    private AudioSource musicSource;
    private Dictionary<string, SoundEffect> sfxDict = new Dictionary<string, SoundEffect>();

    // Índice de la pista actual en backgroundMusics
    private int currentMusicIndex = -1;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.playOnAwake = false;
        musicSource.volume = musicVolume;
        musicSource.spatialBlend = 0f;

        foreach (var sfx in soundEffects)
            if (!string.IsNullOrEmpty(sfx.name))
                sfxDict[sfx.name] = sfx;

        if (backgroundMusics != null && backgroundMusics.Length > 0)
            PlayMusicByIndex(0);
    }

    // ──────────────────────────────────────────
    //  PLAYLIST METHODS
    // ──────────────────────────────────────────

    /// <summary>Reproduce la pista en la posición indicada del array.</summary>
    public void PlayMusicByIndex(int index)
    {
        if (backgroundMusics == null || backgroundMusics.Length == 0) return;
        index = Mathf.Clamp(index, 0, backgroundMusics.Length - 1);
        currentMusicIndex = index;
        PlayMusic(backgroundMusics[currentMusicIndex]);
    }

    public void NextMusic()
    {
        if (backgroundMusics == null || backgroundMusics.Length == 0) return;

        int next = currentMusicIndex + 1;
        if (next >= backgroundMusics.Length)
        {
            Debug.Log("[AudioManager] Ya estás en la última pista.");
            return;
        }

        PlayMusicByIndex(next);
    }

    public void PreviousMusic()
    {
        if (backgroundMusics == null || backgroundMusics.Length == 0) return;

        int prev = currentMusicIndex - 1;
        if (prev < 0)
        {
            Debug.Log("[AudioManager] Ya estás en la primera pista.");
            return;
        }

        PlayMusicByIndex(prev);
    }

    /// <summary>Alterna directamente entre dos índices (útil para combat/exploration music, etc.).</summary>
    public void ToggleMusic(int indexA, int indexB)
    {
        if (currentMusicIndex == indexA)
            PlayMusicByIndex(indexB);
        else
            PlayMusicByIndex(indexA);
    }

    /// <summary>Devuelve el índice de la pista que está sonando actualmente.</summary>
    public int CurrentMusicIndex => currentMusicIndex;

    // ──────────────────────────────────────────
    //  SFX
    // ──────────────────────────────────────────

    public void PlaySFX(string sfxName, Transform spawnTransform)
    {
        if (!sfxDict.TryGetValue(sfxName, out SoundEffect sfx))
        {
            Debug.LogWarning($"[AudioManager] SFX not found: '{sfxName}'");
            return;
        }

        if (sfx.clip == null)
        {
            Debug.LogWarning($"[AudioManager] Clip is null on SFX: '{sfxName}'");
            return;
        }

        Debug.Log($"[AudioManager] Playing SFX: '{sfxName}' at position {spawnTransform.position}");
        AudioSource audioSource = soundFXObject != null
            ? Instantiate(soundFXObject, spawnTransform.position, Quaternion.identity)
            : new GameObject($"SFX_{sfx.clip.name}").AddComponent<AudioSource>();

        audioSource.transform.position = spawnTransform.position;
        audioSource.transform.SetParent(null);
        audioSource.clip = sfx.clip;
        audioSource.volume = sfx.volume * sfxVolume;
        audioSource.pitch = sfx.pitch;
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
        audioSource.Play();
        Debug.Log($"[AudioManager] AudioSource state: clip={audioSource.clip.name}, time={audioSource.time}, isPlaying={audioSource.isPlaying}");
        //Debug.Log($"[AudioManager] AudioSource state: clip={audioSource.clip.name}, time={audioSource.time}, isPlaying={audioSource.isPlaying}");

        Destroy(audioSource.gameObject, audioSource.clip.length);
    }

    public void PlayMiaw(Transform t) => PlaySFX("Miaw", t);
    public void PlayMetalPipe(Transform t) => PlaySFX("MetalPipe", t);
    public void PlayAccelerator(Transform t) => PlaySFX("Accelerator", t);
    public void PlayBoost(Transform t) => PlaySFX("BoostVelocity", t);
    public void PlayCheckpoint(Transform t) => PlaySFX("Checkpoint", t);
    public void PlayEnemyShot(Transform t) => PlaySFX("EnemyShot", t);
    public void PlayShield(Transform t) => PlaySFX("Shield", t);
    public void PlayEnemyDeath(Transform t) => PlaySFX("EnemyDeath", t);
    public void PlayLoseRings(Transform t) => PlaySFX("LoseRings", t);
    public void PlaySpikes(Transform t) => PlaySFX("Spikes", t);
    public void PlayPickEmerald(Transform t) => PlaySFX("PickEmerald", t);
    public void PlayPickRings(Transform t) => PlaySFX("PickRings", t);
    public void PlayRunning(Transform t) => PlaySFX("Running", t);
    public void PlayJump(Transform t) => PlaySFX("Jump", t);
    public void PlayPowerUp(Transform t) => PlaySFX("PowerUp", t);
    public void PlayTrampolines(Transform t) => PlaySFX("Trampolines", t);
    public void PlayWalking(Transform t) => PlaySFX("Walking", t);
    public void PlayEggmanLaugh(Transform t) => PlaySFX("EggmanLaugh", t);

    // ──────────────────────────────────────────
    //  MUSIC CORE
    // ──────────────────────────────────────────

    public void PlayMusic(AudioClip clip)
    {
        musicSource.clip = clip;
        musicSource.volume = musicVolume;
        musicSource.Play();
    }

    public void StopMusic() => musicSource.Stop();
    public void PauseMusic() => musicSource.Pause();
    public void ResumeMusic() => musicSource.UnPause();

    public void SetMusicVolume(float value)
    {
        musicVolume = Mathf.Clamp01(value);
        musicSource.volume = musicVolume;
    }

    public void SetSFXVolume(float value) => sfxVolume = Mathf.Clamp01(value);

    public void PlayWinSequence(Transform t, string sfxName = "PowerUp")
    {
        StartCoroutine(WinSequenceRoutine(t, sfxName));
    }

    private IEnumerator WinSequenceRoutine(Transform t, string sfxName)
    {
        StopMusic();

        if (sfxDict.TryGetValue(sfxName, out SoundEffect sfx) && sfx.clip != null)
        {
            PlaySFX(sfxName, t);
            yield return new WaitForSeconds(sfx.clip.length);
        }

        PlayMusicByIndex(2);
    }
}
