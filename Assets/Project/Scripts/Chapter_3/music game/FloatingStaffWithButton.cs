using UnityEngine;
using System.Collections;

public class FloatingStaffWithButton : MonoBehaviour
{
    [Header("五线谱图片")]
    public RectTransform staffImage;

    [Header("浮动设置")]
    public float floatSpeed = 0.5f;
    public float floatAmplitude = 6f;

    [Header("音符按钮")]
    public CanvasGroup noteButtonContainer;
    public GameObject[] noteButtons;
    public float fadeDuration = 0.8f;

    [Header("背景音乐")]
    public AudioSource bgmAudioSource;
    public float bgmFadeDuration = 1.5f;      // 背景音乐淡出时间

    private Vector3 originalPosition;
    private bool isUnlocked = false;
    private Coroutine fadeCoroutine;
    private Vector3[] noteButtonStartPos;
    private Coroutine bgmFadeCoroutine;

    void Start()
    {
        if (staffImage != null)
            originalPosition = staffImage.localPosition;

        // 记录音符按钮初始位置
        if (noteButtons != null && noteButtons.Length > 0)
        {
            if (noteButtonContainer != null)
            {
                bool wasActive = noteButtonContainer.gameObject.activeSelf;
                noteButtonContainer.gameObject.SetActive(true);

                noteButtonStartPos = new Vector3[noteButtons.Length];
                for (int i = 0; i < noteButtons.Length; i++)
                {
                    if (noteButtons[i] != null)
                        noteButtonStartPos[i] = noteButtons[i].transform.localPosition;
                }

                noteButtonContainer.gameObject.SetActive(false);
                noteButtonContainer.alpha = 0f;
            }
        }

        if (noteButtonContainer != null)
        {
            noteButtonContainer.alpha = 0f;
            noteButtonContainer.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (staffImage != null)
        {
            float offset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
            staffImage.localPosition = originalPosition + new Vector3(0, offset, 0);
        }

        if (isUnlocked && noteButtons != null && noteButtons.Length > 0)
        {
            FloatNoteButtons();
        }
    }

    void FloatNoteButtons()
    {
        float time = Time.time * floatSpeed;
        for (int i = 0; i < noteButtons.Length; i++)
        {
            if (noteButtons[i] == null) continue;

            float direction = (i % 2 == 0) ? -1f : 1f;
            float phase = i * 0.9f;
            float offset = Mathf.Sin(time + phase) * floatAmplitude * direction;

            Vector3 pos = noteButtonStartPos[i];
            pos.y += offset;
            noteButtons[i].transform.localPosition = pos;
        }
    }

    // 点击五线谱调用此方法
    public void OnStaffClick()
    {
        if (isUnlocked) return;
        isUnlocked = true;

        // 🎵 背景音乐渐变停止
        if (bgmAudioSource != null && bgmAudioSource.isPlaying)
        {
            if (bgmFadeCoroutine != null)
                StopCoroutine(bgmFadeCoroutine);
            bgmFadeCoroutine = StartCoroutine(FadeOutBGM());
        }

        // 显示音符按钮
        if (noteButtonContainer != null)
        {
            if (fadeCoroutine != null)
                StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeInButtons());
        }
    }

    // 🎵 背景音乐淡出协程
    IEnumerator FadeOutBGM()
    {
        float elapsed = 0f;
        float startVolume = bgmAudioSource.volume;

        while (elapsed < bgmFadeDuration)
        {
            elapsed += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, 0f, elapsed / bgmFadeDuration);
            bgmAudioSource.volume = newVolume;
            yield return null;
        }

        bgmAudioSource.volume = 0f;
        bgmAudioSource.Stop();
        // 重置音量，以备后续重新使用
        bgmAudioSource.volume = startVolume;
        bgmFadeCoroutine = null;

        Debug.Log("背景音乐淡出完成");
    }

    IEnumerator FadeInButtons()
    {
        noteButtonContainer.gameObject.SetActive(true);
        noteButtonContainer.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            noteButtonContainer.alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            yield return null;
        }

        noteButtonContainer.alpha = 1f;
        fadeCoroutine = null;
    }
}