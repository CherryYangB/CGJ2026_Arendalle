using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ClickFeedback : MonoBehaviour, IPointerClickHandler
{
    [Header("音效")]
    public AudioClip clickSound;
    public AudioSource audioSource;

    [Header("底部介绍文字")]
    public TextMeshProUGUI bottomText;   
    
    [TextArea(2, 4)]
    public string displayText = "点击了物品";   // 点击时显示的文字

    [Header("设置")]
    public bool playSoundOnClick = true;
    public bool updateTextOnClick = true;

    void Start()
    {
        // 如果底部文字有初始内容，可以保留
        // 如果没有 AudioSource，尝试自动获取或创建
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
        if (audioSource == null && clickSound != null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    // IPointerClickHandler 接口方法
    public void OnPointerClick(PointerEventData eventData)
    {
        // 播放音效
        if (playSoundOnClick && audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
            Debug.Log("ClickFeedback: 播放音效");
        }

        // 更新底部文字
        if (updateTextOnClick && bottomText != null)
        {
            bottomText.text = displayText;
            Debug.Log("ClickFeedback: 更新文字为: " + displayText);
        }
    }

    // 公开方法：允许外部手动触发反馈（比如从其他脚本调用）
    public void TriggerFeedback()
    {
        if (playSoundOnClick && audioSource != null && clickSound != null)
        {
            audioSource.PlayOneShot(clickSound);
        }
        if (updateTextOnClick && bottomText != null)
        {
            bottomText.text = displayText;
        }
    }
}
