using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;  // ← 必须引用 TextMeshPro 命名空间

public class ClickToShow : MonoBehaviour, IPointerClickHandler
{
    [Header("目标大模块")]
    public GameObject targetBigModule;

    [Header("音效")]
    public AudioClip clickSound;
    public AudioSource audioSource;

    [Header("底部文字（使用 TextMeshPro）")]
    public TextMeshProUGUI bottomText;   // ← 改为 TMP 类型
    [TextArea(2, 4)]
    public string displayText = "物品介绍";

    [Header("大模块管理")]
    public BigModuleManager moduleManager;

    void Start()
    {
        if (moduleManager == null)
            moduleManager = FindObjectOfType<BigModuleManager>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null && clickSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 1. 播放音效
        if (audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
            Debug.Log("播放音效: " + clickSound.name);
        }

        // 2. 打开大模块
        if (moduleManager != null && targetBigModule != null)
        {
            moduleManager.OpenModule(targetBigModule);
        }
        else if (targetBigModule != null)
        {
            targetBigModule.SetActive(true);
        }

        // 3. 更新底部文字
        if (bottomText != null)
        {
            bottomText.text = displayText;
            Debug.Log("底部文字更新为: " + displayText);
        }
    }
}