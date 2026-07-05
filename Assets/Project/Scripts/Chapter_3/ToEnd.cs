using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ClickAnywhereToEnd : MonoBehaviour
{
    [Header("目标场景")]
    public string targetScene = "EndScene";

    [Header("BGM淡出")]
    public AudioSource bgmAudioSource;           // 拖入第二页的BGM AudioSource
    public float bgmFadeDuration = 1.5f;         // BGM淡出时间

    [Header("白色遮罩")]
    public float fadeInDuration = 1.0f;          // 渐白时间
    public float holdWhiteDuration = 0.5f;       // 全白停留
    public float fadeOutDuration = 0.8f;         // 淡出时间（加载场景后）

    private bool isTransitioning = false;
    private GameObject fadePanel;
    private CanvasGroup fadeCanvasGroup;

    void Update()
    {
        // 点击屏幕任意位置触发
        if ((Input.GetMouseButtonDown(0) || Input.touchCount > 0) && !isTransitioning)
        {
            StartCoroutine(TransitionToEnd());
        }
    }

    IEnumerator TransitionToEnd()
    {
        isTransitioning = true;

        // 1. 创建白色遮罩（在 Canvas 下）
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("场景中没有 Canvas！");
            yield break;
        }

        fadePanel = new GameObject("WhiteFade");
        fadePanel.transform.SetParent(canvas.transform, false);
        fadePanel.transform.SetAsLastSibling();

        // 添加 Image 组件，设置为白色
        Image img = fadePanel.AddComponent<Image>();
        img.color = Color.white;
        img.raycastTarget = false;

        // 添加 CanvasGroup，铺满全屏
        RectTransform rect = fadePanel.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;

        fadeCanvasGroup = fadePanel.AddComponent<CanvasGroup>();
        fadeCanvasGroup.alpha = 0f;
        fadeCanvasGroup.blocksRaycasts = true;
        fadeCanvasGroup.interactable = true;

        // 2. BGM淡出（如果有）
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            float startVol = bgmAudioSource.volume;
            float elapsed = 0f;
            while (elapsed < bgmFadeDuration)
            {
                elapsed += Time.deltaTime;
                bgmAudioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / bgmFadeDuration);
                yield return null;
            }
            bgmAudioSource.volume = 0f;
            bgmAudioSource.Stop();
        }

        // 3. 渐白
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
            yield return null;
        }
        fadeCanvasGroup.alpha = 1f;

        // 4. 全白停留
        yield return new WaitForSeconds(holdWhiteDuration);

        // 5. 保留遮罩跨场景
        DontDestroyOnLoad(fadePanel);

        // 6. 加载目标场景
        SceneManager.LoadScene(targetScene);

        // 7. 等待场景加载完成（一帧）
        yield return null;

        // 8. 淡出遮罩（在新场景中）
        t = 0f;
        while (t < fadeOutDuration)
        {
            t += Time.deltaTime;
            if (fadeCanvasGroup != null)
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
            yield return null;
        }
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            fadeCanvasGroup.interactable = false;
        }

        // 9. 延迟销毁遮罩（避免过早消失）
        yield return new WaitForSeconds(0.2f);
        if (fadePanel != null)
            Destroy(fadePanel);

        isTransitioning = false;
        Debug.Log("✅ 过渡到 End 场景完成");
    }
}