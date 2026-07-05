using UnityEngine;
using System.Collections;

public class EndSceneWhiteTransition : MonoBehaviour
{
    [Header("白色遮罩")]
    public CanvasGroup whiteMask;              // 白色遮罩的 CanvasGroup

    [Header("结局图片（可选）")]
    public CanvasGroup targetImage;            // 结局图片（如果初始需要隐藏）

    [Header("过渡设置")]
    public float fadeOutDuration = 2.5f;       // 白色淡出持续时间
    public float holdWhiteDuration = 0.5f;     // 停留白色再开始淡出

    void Start()
    {
        // 确保白色遮罩完全覆盖
        if (whiteMask != null)
        {
            whiteMask.alpha = 1f;
            whiteMask.gameObject.SetActive(true);
        }

        // 如果设置了目标图片，确保它可见
        if (targetImage != null)
        {
            targetImage.alpha = 1f;
        }

        StartCoroutine(Transition());
    }

    IEnumerator Transition()
    {
        // 短暂停留白色
        yield return new WaitForSeconds(holdWhiteDuration);

        // 白色遮罩缓慢淡出，露出图片
        float elapsed = 0f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            if (whiteMask != null)
            {
                whiteMask.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            }
            yield return null;
        }

        if (whiteMask != null)
        {
            whiteMask.alpha = 0f;
            whiteMask.gameObject.SetActive(false);
        }

        Debug.Log("✅ 白色过渡完成，图片已显示");
    }
}