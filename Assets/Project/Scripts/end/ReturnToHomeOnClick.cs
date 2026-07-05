using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ReturnToHomeOnClick : MonoBehaviour
{
    [Header("目标场景")]
    public string homeSceneName = "Home";

    [Header("第四张图（用于判断是否激活）")]
    public GameObject image4;   // 拖入第四张图对象

    [Header("BGM 淡出")]
    public AudioSource bgmAudioSource;
    public float bgmFadeDuration = 1.5f;

    [Header("白色遮罩")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeInDuration = 1.0f;
    public float fadeOutDuration = 0.8f;
    public float holdWhiteDuration = 0.5f;

    private bool isTransitioning = false;

    void Update()
    {
        // 如果正在过渡，或第四张图未激活，则忽略点击
        if (isTransitioning) return;
        if (image4 == null || !image4.activeSelf) return;

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            StartCoroutine(TransitionToHome());
        }
    }

    IEnumerator TransitionToHome()
    {
        isTransitioning = true;

        // 1. BGM 淡出
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

        // 2. 渐白
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = true;

            float elapsed = 0f;
            while (elapsed < fadeInDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeInDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(holdWhiteDuration);

        // 3. 保留遮罩跨场景
        if (fadeCanvasGroup != null)
        {
            DontDestroyOnLoad(fadeCanvasGroup.gameObject);
        }

        // 4. 加载目标场景
        SceneManager.LoadScene(homeSceneName);

        // 5. 等待场景加载完成（一帧）
        yield return null;

        // 6. 在新场景中淡出遮罩
        if (fadeCanvasGroup != null)
        {
            float elapsed = 0f;
            while (elapsed < fadeOutDuration)
            {
                elapsed += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.blocksRaycasts = false;
            Destroy(fadeCanvasGroup.gameObject, 0.2f);
        }

        isTransitioning = false;
    }
}