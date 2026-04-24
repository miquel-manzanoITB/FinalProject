using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Plays a fade-to-black (or any color) transition when loading scenes.
/// Attach to a Canvas that sits in front of everything.
/// Call SceneTransition.Instance.FadeOut() before loading, FadeIn() after.
///
/// GameManager.LoadScene() triggers this automatically if it finds the instance.
/// </summary>
public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [Header("Fade")]
    public Image fadeImage;
    public float fadeDuration = 0.4f;
    public Color fadeColor = Color.black;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        if (fadeImage != null)
        {
            //fadeImage.color = fadeColor with { a = 1f };
            FadeIn();
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Fades screen in (clear). Call after a scene loads.</summary>
    public void FadeIn() => StartCoroutine(Fade(1f, 0f));

    /// <summary>Fades screen out (black). Call before loading a scene.</summary>
    public Coroutine FadeOut() => StartCoroutine(Fade(0f, 1f));

    /// <summary>Convenience: fade out, load scene, fade in.</summary>
    public void TransitionTo(string sceneName)
        => StartCoroutine(TransitionCoroutine(sceneName));

    // ── Coroutines ────────────────────────────────────────────────────────────

    IEnumerator TransitionCoroutine(string sceneName)
    {
        yield return Fade(0f, 1f);
        GameManager.Instance.LoadScene(sceneName);
        // FadeIn is called from Awake on the next scene (or from GameManager callback)
    }

    IEnumerator Fade(float fromAlpha, float toAlpha)
    {
        if (fadeImage == null) yield break;
        fadeImage.gameObject.SetActive(true);
        float t = 0f;
        Color c = fadeColor;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            c.a = Mathf.Lerp(fromAlpha, toAlpha, t / fadeDuration);
            fadeImage.color = c;
            yield return null;
        }
        c.a = toAlpha;
        fadeImage.color = c;
        if (toAlpha == 0f) fadeImage.gameObject.SetActive(false);
    }
}