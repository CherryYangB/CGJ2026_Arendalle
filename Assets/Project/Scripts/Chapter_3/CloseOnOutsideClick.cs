using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CloseOnOutsideClick : MonoBehaviour
{
    [Header("点击检测配置")]
    public bool enableOutsideClose = true;
    public bool closeAll = true;

    void Update()
    {
        if (!gameObject.activeSelf || !enableOutsideClose) return;

        if (Input.GetMouseButtonDown(0))
        {
            // 【方法一】先判断点击是否在本模块上（最可靠）
            if (IsPointerOverThisModule())
            {
                // 点击在本模块内部 → 不做任何事
                return;
            }

            // 点击在本模块外部 → 关闭
            CloseModule();
        }
    }

    // 检测鼠标是否在本模块或子对象上
    private bool IsPointerOverThisModule()
    {
        Vector2 mousePos = Input.mousePosition;

        var raycastResults = new List<RaycastResult>();
        var pointerData = new PointerEventData(EventSystem.current)
        {
            position = mousePos
        };
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            if (result.gameObject == this.gameObject ||
                result.gameObject.transform.IsChildOf(this.transform))
            {
                return true;
            }
        }
        return false;
    }

    void CloseModule()
    {
        Debug.Log("CloseModule 被调用！关闭大模块");
        if (closeAll)
        {
            if (BigModuleManager.Instance != null)
                BigModuleManager.Instance.CloseAll();
        }
        else
        {
            this.gameObject.SetActive(false);
        }
    }
}