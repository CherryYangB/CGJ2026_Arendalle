using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NoteSequencePuzzle : MonoBehaviour
{
    [Header("音符按钮")]
    public GameObject[] noteButtons;              // 9个音符按钮
    public AudioSource audioSource;               // 用于播放音效

    [Header("正确顺序（在 Inspector 中设置）")]
    public int[] correctSequence = { 3, 5, 1, 7, 9, 2, 4, 6, 8 };

    [Header("每个音符对应的音效")]
    public AudioClip[] noteSounds;                // 9个音效

    [Header("成功反馈")]
    public AudioClip successSound;
    public float successDelay = 1f;

    [Header("错误反馈")]
    public AudioClip wrongSound;
    public float alphaStep = 0.1f;
    public float saturationStep = 0.1f;
    public float minAlpha = 0.25f;
    public float minSaturation = 0.25f;

    [Header("翻页过渡")]
    public DiaryPageTransition pageTransition;    // ← 这个字段就是 Page Transition

    [Header("动画设置")]
    public float fadeDuration = 0.6f;
    public float resetDelay = 0.8f;

    private List<int> clickedSequence = new List<int>();
    private bool isPuzzleComplete = false;
    private bool isResetting = false;

    private float currentAlpha = 1f;
    private float currentSaturation = 1f;

    private CanvasGroup[] noteCanvasGroups;
    private Color[] initialNoteColors;

    void Start()
    {
        noteCanvasGroups = new CanvasGroup[noteButtons.Length];
        initialNoteColors = new Color[noteButtons.Length];

        for (int i = 0; i < noteButtons.Length; i++)
        {
            var cg = noteButtons[i].GetComponent<CanvasGroup>();
            if (cg == null)
                cg = noteButtons[i].AddComponent<CanvasGroup>();
            noteCanvasGroups[i] = cg;
            cg.alpha = 1f;

            var img = noteButtons[i].GetComponent<UnityEngine.UI.Image>();
            if (img != null)
                initialNoteColors[i] = img.color;
            else
                initialNoteColors[i] = Color.white;
        }

        currentAlpha = 1f;
        currentSaturation = 1f;

        for (int i = 0; i < noteButtons.Length; i++)
        {
            int index = i;
            var btn = noteButtons[i]?.GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => OnNoteClick(index));
            }
        }
    }

    void OnNoteClick(int index)
    {
        if (isPuzzleComplete || isResetting) return;
        if (!noteButtons[index].activeSelf) return;

        int noteNumber = index + 1;
        int expectedIndex = clickedSequence.Count;

        if (expectedIndex >= correctSequence.Length) return;

        if (noteNumber == correctSequence[expectedIndex])
        {
            StartCoroutine(CorrectNote(index));
        }
        else
        {
            StartCoroutine(WrongReset());
        }
    }

    IEnumerator CorrectNote(int index)
    {
        if (audioSource != null && noteSounds != null && noteSounds.Length > index)
        {
            audioSource.PlayOneShot(noteSounds[index]);
        }

        clickedSequence.Add(index + 1);
        yield return StartCoroutine(FadeOutNote(noteButtons[index]));

        if (clickedSequence.Count == correctSequence.Length)
        {
            yield return new WaitForSeconds(successDelay);
            OnPuzzleComplete();
        }
    }

    IEnumerator FadeOutNote(GameObject note)
    {
        var cg = note.GetComponent<CanvasGroup>();
        float elapsed = 0f;
        float startAlpha = cg.alpha;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
        note.SetActive(false);
    }

    IEnumerator WrongReset()
    {
        isResetting = true;

        if (audioSource != null && wrongSound != null)
        {
            audioSource.PlayOneShot(wrongSound);
        }

        clickedSequence.Clear();

        currentAlpha = Mathf.Max(minAlpha, currentAlpha - alphaStep);
        currentSaturation = Mathf.Max(minSaturation, currentSaturation - saturationStep);

        ApplyAlphaAndSaturationToAll(currentAlpha, currentSaturation);

        Debug.Log($"透明度降至: {currentAlpha}, 饱和度降至: {currentSaturation}");

        foreach (var note in noteButtons)
        {
            if (!note.activeSelf)
            {
                note.SetActive(true);
                var cg = note.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    cg.alpha = 0f;
                    StartCoroutine(FadeInNote(note, currentAlpha));
                }
            }
            else
            {
                var cg = note.GetComponent<CanvasGroup>();
                if (cg != null)
                {
                    StartCoroutine(FlashNote(note, currentAlpha));
                }
            }
        }

        yield return new WaitForSeconds(resetDelay + fadeDuration);
        isResetting = false;
    }

    void ApplyAlphaAndSaturationToAll(float alpha, float saturation)
    {
        for (int i = 0; i < noteCanvasGroups.Length; i++)
        {
            if (noteCanvasGroups[i] != null)
                noteCanvasGroups[i].alpha = alpha;
        }

        for (int i = 0; i < noteButtons.Length; i++)
        {
            var img = noteButtons[i]?.GetComponent<UnityEngine.UI.Image>();
            if (img != null && i < initialNoteColors.Length)
            {
                Color c = initialNoteColors[i];
                float gray = (c.r + c.g + c.b) / 3f;
                c.r = Mathf.Lerp(gray, c.r, saturation);
                c.g = Mathf.Lerp(gray, c.g, saturation);
                c.b = Mathf.Lerp(gray, c.b, saturation);
                img.color = c;
            }
        }
    }

    IEnumerator FadeInNote(GameObject note, float targetAlpha)
    {
        var cg = note.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

    IEnumerator FlashNote(GameObject note, float baseAlpha)
    {
        var cg = note.GetComponent<CanvasGroup>();
        for (int i = 0; i < 2; i++)
        {
            cg.alpha = baseAlpha * 0.3f;
            yield return new WaitForSeconds(0.15f);
            cg.alpha = baseAlpha;
            yield return new WaitForSeconds(0.15f);
        }
    }

    void OnPuzzleComplete()
    {
        isPuzzleComplete = true;

        if (audioSource != null && successSound != null)
        {
            audioSource.PlayOneShot(successSound);
        }

        Debug.Log("🎵 解密成功！");

        // ===== 触发翻页过渡 =====
        if (pageTransition != null)
        {
            Debug.Log("✅ 使用 Inspector 拖入的 PageTransition");
            pageTransition.StartPageTransition();
        }
        else
        {
            Debug.Log("⚠️ pageTransition 字段为空，尝试自动查找...");
            DiaryPageTransition found = FindObjectOfType<DiaryPageTransition>();
            if (found != null)
            {
                Debug.Log("✅ 自动找到 DiaryPageTransition: " + found.gameObject.name);
                found.StartPageTransition();
            }
            else
            {
                Debug.LogError("❌ 未找到 DiaryPageTransition 组件！请确保已挂载脚本。");
            }
        }
    }

    public void ResetPuzzle(bool resetAlphaAndSaturation = true)
    {
        if (isResetting) return;
        if (resetAlphaAndSaturation)
        {
            currentAlpha = 1f;
            currentSaturation = 1f;
            ApplyAlphaAndSaturationToAll(1f, 1f);
        }
        StartCoroutine(WrongReset());
        isPuzzleComplete = false;
    }

    public int GetProgress()
    {
        return clickedSequence.Count;
    }

    public bool IsPuzzleComplete()
    {
        return isPuzzleComplete;
    }
}