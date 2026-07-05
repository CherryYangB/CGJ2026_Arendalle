using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EndSceneController : MonoBehaviour
{
    [Header("四张图片")]
    public Image image1, image2, image3, image4;

    [Header("顶部对话（图2显示）")]
    public Text dialogueLine1, dialogueLine2;
    public float dialogueDuration = 3f;

    [Header("图三信息文字")]
    public Text infoText;
    public string infoTextContent = "全球约有5500万阿尔茨海默症患者，每3秒新增一例。";
    public float infoTextDisplayDuration = 4f;

    [Header("结束文字（两行）")]
    public Text endTextLine1, endTextLine2;

    [Header("白色遮罩")]
    public CanvasGroup fadeCanvasGroup;
    public float fadeInDuration = 1.0f, fadeOutDuration = 0.8f, holdWhiteDuration = 0.5f;

    [Header("图1→图2 切换")]
    public float autoSwitchDelay = 5f;

    [Header("图2→图3 切换")]
    public float autoSwitchToThirdDelay = 10f;   // 图2对话结束后等待10秒自动切图3
    public float crossFadeDuration = 1.5f;

    [Header("图3→图4 淡入")]
    public float switchToFourthDelay = 5f;
    public float fourthFadeDuration = 1.5f;
    public float line2Delay = 1.5f;

    [Header("音效")]
    public AudioSource audioSource;

    private bool isTransitioning = false;
    private bool isSecondImageShown = false;
    private bool isThirdImageShown = false;
    private bool isFourthImageShown = false;

    private int dialogueStep = 0;
    private bool skipToNext = false;
    private Coroutine autoSwitchCoroutine;
    private Coroutine autoThirdCoroutine;   // 自动切图3的协程引用

    void Start()
    {
        image1.gameObject.SetActive(true);
        image2.gameObject.SetActive(false);
        image3.gameObject.SetActive(false);
        image4.gameObject.SetActive(false);
        dialogueLine1.gameObject.SetActive(false);
        dialogueLine2.gameObject.SetActive(false);
        infoText.gameObject.SetActive(false);
        endTextLine1.gameObject.SetActive(false);
        endTextLine2.gameObject.SetActive(false);

        EnsureCanvasGroup(image2);
        EnsureCanvasGroup(image3);
        EnsureCanvasGroup(image4);

        if (fadeCanvasGroup != null)
            fadeCanvasGroup.gameObject.SetActive(false);

        Invoke("AutoSwitch", autoSwitchDelay);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // 图1→图2切换（尚未显示图2时）
            if (!isSecondImageShown && !isTransitioning)
            {
                CancelInvoke("AutoSwitch");
                StartSwitch();
                return;
            }

            // 图2对话控制
            if (isSecondImageShown && !isThirdImageShown && !isTransitioning)
            {
                if (dialogueStep == 1)
                    skipToNext = true;
                else if (dialogueStep == 2)
                    skipToNext = true;
                else if (dialogueStep == 3)
                {
                    // 点击跳过自动等待，立即切图3
                    if (autoThirdCoroutine != null)
                    {
                        StopCoroutine(autoThirdCoroutine);
                        autoThirdCoroutine = null;
                    }
                    StartCoroutine(FadeToThird());
                }
            }
        }
    }

    void AutoSwitch() => StartSwitch();

    void StartSwitch()
    {
        if (isTransitioning) return;
        StartCoroutine(SwitchToSecond());
    }

    IEnumerator SwitchToSecond()
    {
        isTransitioning = true;

        if (audioSource != null && audioSource.clip != null)
            audioSource.PlayOneShot(audioSource.clip);

        // 渐白
        if (fadeCanvasGroup != null)
        {
            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = 0f;
            float t = 0f;
            while (t < fadeInDuration)
            {
                t += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / fadeInDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 1f;
        }

        yield return new WaitForSeconds(holdWhiteDuration);

        image1.gameObject.SetActive(false);
        image2.gameObject.SetActive(true);
        isSecondImageShown = true;

        if (fadeCanvasGroup != null)
        {
            float t = 0f;
            while (t < fadeOutDuration)
            {
                t += Time.deltaTime;
                fadeCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutDuration);
                yield return null;
            }
            fadeCanvasGroup.alpha = 0f;
            fadeCanvasGroup.gameObject.SetActive(false);
        }

        isTransitioning = false;

        if (dialogueLine1 != null && dialogueLine2 != null)
            StartCoroutine(ShowDialogue());
    }

    IEnumerator ShowDialogue()
    {
        dialogueStep = 0;
        dialogueLine1.gameObject.SetActive(false);
        dialogueLine2.gameObject.SetActive(false);

        // 第一行
        dialogueLine1.gameObject.SetActive(true);
        dialogueStep = 1;
        skipToNext = false;
        float timer = 0f;
        while (timer < dialogueDuration && !skipToNext)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        dialogueLine1.gameObject.SetActive(false);

        // 第二行
        dialogueLine2.gameObject.SetActive(true);
        dialogueStep = 2;
        skipToNext = false;
        timer = 0f;
        while (timer < dialogueDuration && !skipToNext)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        dialogueLine2.gameObject.SetActive(false);

        dialogueStep = 3; // 对话结束

        // 启动自动切换图3的计时器
        if (autoThirdCoroutine != null)
            StopCoroutine(autoThirdCoroutine);
        autoThirdCoroutine = StartCoroutine(AutoTransitionToThird());
    }

    IEnumerator AutoTransitionToThird()
    {
        yield return new WaitForSeconds(autoSwitchToThirdDelay);

        // 如果还没有切换到图3，且不在过渡中，则自动切换
        if (!isThirdImageShown && !isTransitioning)
        {
            StartCoroutine(FadeToThird());
        }
        autoThirdCoroutine = null;
    }

    IEnumerator FadeToThird()
    {
        if (isTransitioning) yield break;
        if (isThirdImageShown) yield break;

        // 停止自动计时器，防止重复触发
        if (autoThirdCoroutine != null)
        {
            StopCoroutine(autoThirdCoroutine);
            autoThirdCoroutine = null;
        }

        isTransitioning = true;

        image3.gameObject.SetActive(true);
        CanvasGroup cg3 = image3.GetComponent<CanvasGroup>();
        if (cg3 == null) cg3 = image3.gameObject.AddComponent<CanvasGroup>();
        cg3.alpha = 0f;

        CanvasGroup cg2 = image2.GetComponent<CanvasGroup>();
        if (cg2 == null) cg2 = image2.gameObject.AddComponent<CanvasGroup>();
        cg2.alpha = 1f;

        float t = 0f;
        while (t < crossFadeDuration)
        {
            t += Time.deltaTime;
            float progress = t / crossFadeDuration;
            cg2.alpha = 1f - progress;
            cg3.alpha = progress;
            yield return null;
        }

        cg2.alpha = 0f;
        cg3.alpha = 1f;
        isThirdImageShown = true;
        isTransitioning = false;

        // 启动图三文字显示，结束后自动图四
        StartCoroutine(ShowInfoAndDelay());
    }

    IEnumerator ShowInfoAndDelay()
    {
        if (infoText != null)
        {
            infoText.gameObject.SetActive(true);
            infoText.text = infoTextContent;
        }

        yield return new WaitForSeconds(infoTextDisplayDuration);

        if (infoText != null)
            infoText.gameObject.SetActive(false);

        float remaining = Mathf.Max(0, switchToFourthDelay - infoTextDisplayDuration);
        if (remaining > 0)
            yield return new WaitForSeconds(remaining);

        if (!isFourthImageShown)
            StartCoroutine(WaitAndFadeToFourth());
    }

    IEnumerator WaitAndFadeToFourth()
    {
        if (isFourthImageShown || isTransitioning) yield break;

        isTransitioning = true;

        image4.gameObject.SetActive(true);
        endTextLine1.gameObject.SetActive(true);
        endTextLine2.gameObject.SetActive(true);

        CanvasGroup cg4 = image4.GetComponent<CanvasGroup>();
        if (cg4 == null) cg4 = image4.gameObject.AddComponent<CanvasGroup>();
        cg4.alpha = 0f;

        Color c1 = endTextLine1.color;
        c1.a = 0f;
        endTextLine1.color = c1;

        Color c2 = endTextLine2.color;
        c2.a = 0f;
        endTextLine2.color = c2;

        float t = 0f;
        while (t < fourthFadeDuration)
        {
            t += Time.deltaTime;
            float progress = t / fourthFadeDuration;
            cg4.alpha = progress;
            c1.a = progress;
            endTextLine1.color = c1;
            yield return null;
        }

        cg4.alpha = 1f;
        c1.a = 1f;
        endTextLine1.color = c1;

        yield return new WaitForSeconds(line2Delay);

        t = 0f;
        while (t < fourthFadeDuration * 0.6f)
        {
            t += Time.deltaTime;
            float progress = t / (fourthFadeDuration * 0.6f);
            c2.a = Mathf.Lerp(0f, 1f, progress);
            endTextLine2.color = c2;
            yield return null;
        }

        c2.a = 1f;
        endTextLine2.color = c2;

        isFourthImageShown = true;
        isTransitioning = false;
    }

    void EnsureCanvasGroup(Image img)
    {
        if (img != null && img.GetComponent<CanvasGroup>() == null)
            img.gameObject.AddComponent<CanvasGroup>();
    }
}