using UnityEngine;
using System.Collections;

public class DiaryPageTransition : MonoBehaviour
{
    [Header("页面切换")]
    public GameObject currentPage;
    public GameObject nextPage;
    public GameObject bigModuleToHide;

    [Header("渐白效果")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeInDuration = 1.2f;
    public float fadeOutDuration = 1.0f;
    public float holdWhiteDuration = 0.8f;

    [Header("五线谱图片（渐白时保留，然后淡出）")]
    public GameObject staffImage;
    public float staffFadeDuration = 0.5f;

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

        // BGM淡入
        if (bgmAudioSource != null && newBGM != null)
        {
            StartCoroutine(SwitchBGM());
        }

        // 准备白色遮罩
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.transform.SetAsLastSibling();
            fadeCanvasGroup.alpha = 0f;
        }

        // 将五线谱提升到Canvas顶层
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) canvas = FindObjectOfType<Canvas>();

        if (staffImage != null && staffImage.activeInHierarchy)
        {
            staffImage.transform.SetParent(canvas.transform, true);
            staffImage.transform.SetAsLastSibling();
            Debug.Log("✅ 五线谱已提升到 Canvas 顶层");
        }

        // 渐白
        yield return StartCoroutine(FadeToWhite());

        // 全白停留，同时五线谱淡出
        CanvasGroup staffCG = staffImage.GetComponent<CanvasGroup>();
        if (staffCG == null && staffImage != null)
            staffCG = staffImage.AddComponent<CanvasGroup>();
        if (staffCG != null) staffCG.alpha = 1f;

        float elapsed = 0f;
        float safeFadeDuration = Mathf.Min(staffFadeDuration, holdWhiteDuration);
        while (elapsed < holdWhiteDuration)
        {
            elapsed += Time.deltaTime;
            if (staffCG != null && elapsed <= safeFadeDuration)
            {
                staffCG.alpha = Mathf.Lerp(1f, 0f, elapsed / safeFadeDuration);
            }
            else if (staffCG != null)
            {
                staffCG.alpha = 0f;
            }
            yield return null;
        }
        if (staffCG != null) staffCG.alpha = 0f;
        if (staffImage != null) staffImage.SetActive(false);

        // 切换页面
        if (currentPage != null) currentPage.SetActive(false);
        if (bigModuleToHide != null) bigModuleToHide.SetActive(false);
        if (nextPage != null) nextPage.SetActive(true);

        // 白色淡出
        yield return StartCoroutine(FadeFromWhite());

        isTransitioning = false;
        Debug.Log("📖 页面切换完成！");
    }

    IEnumerator FadeToWhite()
    {
        if (fadeCanvasGroup == null) yield break;
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
            bgmAudioSource.Stop();

        bgmAudioSource.clip = newBGM;
        bgmAudioSource.loop = true;
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
        Debug.Log("🎵 新BGM淡入完成");
    }
}