using UnityEngine;
using System.Collections;

public class DiaryPageTransition : MonoBehaviour
{
    [Header("页面切换")]
    public GameObject currentPage;
    public GameObject nextPage;

    [Header("渐白效果")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeInDuration = 0.8f;
    public float fadeOutDuration = 0.8f;
    public float holdWhiteDuration = 0.5f;

    [Header("音效")]
    public AudioSource sfxAudioSource;
    public AudioClip pageFlipSound;

    [Header("背景音乐")]
    public AudioSource bgmAudioSource;
    public AudioClip newBGM;
    public float bgmFadeInDuration = 1.5f;

    private bool isTransitioning = false;

    public void StartPageTransition()
    {
        if (isTransitioning) return;
        StartCoroutine(TransitionCoroutine());
    }

    IEnumerator TransitionCoroutine()
    {
        isTransitioning = true;

        yield return StartCoroutine(FadeToWhite());

        if (sfxAudioSource != null && pageFlipSound != null)
        {
            sfxAudioSource.PlayOneShot(pageFlipSound);
        }

        yield return new WaitForSeconds(holdWhiteDuration);

        if (currentPage != null) currentPage.SetActive(false);
        if (nextPage != null) nextPage.SetActive(true);

        yield return StartCoroutine(FadeFromWhite());

        if (bgmAudioSource != null && newBGM != null)
        {
            StartCoroutine(SwitchBGM());
        }

        isTransitioning = false;
        Debug.Log("📖 页面切换完成！");
    }

    IEnumerator FadeToWhite()
    {
        if (fadeCanvasGroup == null) yield break;

        fadeCanvasGroup.gameObject.SetActive(true);
        fadeCanvasGroup.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;
    }

    IEnumerator FadeFromWhite()
    {
        if (fadeCanvasGroup == null) yield break;

        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.gameObject.SetActive(false);
    }

    IEnumerator SwitchBGM()
    {
        if (bgmAudioSource.isPlaying)
        {
            bgmAudioSource.Stop();
        }

        bgmAudioSource.clip = newBGM;
        bgmAudioSource.volume = 0f;
        bgmAudioSource.Play();

        float elapsed = 0f;
        while (elapsed < bgmFadeInDuration)
        {
            elapsed += Time.deltaTime;
            bgmAudioSource.volume = Mathf.Lerp(0f, 1f, elapsed / bgmFadeInDuration);
            yield return null;
        }
        bgmAudioSource.volume = 1f;

        Debug.Log("🎵 新背景音乐已开始");
    }

    public void OnPuzzleComplete()
    {
        StartPageTransition();
    }
}