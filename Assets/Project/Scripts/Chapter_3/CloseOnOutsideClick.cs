using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CloseOnOutsideClick : MonoBehaviour
{
    [Header("点击检测配置")]
    [Tooltip("是否启用点击外部关闭")]
    public bool enableOutsideClose = true;

    [Tooltip("点击外部时，是否关闭所有大模块（true=关闭所有，false=只关闭当前）")]
    public bool closeAll = true;

    void Update()
    {
        // 如果模块本身没有激活，不需要检测
        if (!gameObject.activeSelf) return;
        if (!enableOutsideClose) return;

        // 检测鼠标点击（左键）
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePos = Input.mousePosition;

            // 检查点击是否在 UI 上
            if (EventSystem.current.IsPointerOverGameObject())
            {
                // 点击在 UI 上，检查是否点击在本模块或其子对象上
                var raycastResults = new List<RaycastResult>();
                var pointerData = new PointerEventData(EventSystem.current)
                {
                    position = mousePos
                };
                EventSystem.current.RaycastAll(pointerData, raycastResults);

                bool clickedOnSelf = false;
                foreach (var result in raycastResults)
                {
                    // 检查点击的 GameObject 是否属于本模块（包括子对象）
                    if (result.gameObject == this.gameObject ||
                        result.gameObject.transform.IsChildOf(this.transform))
                    {
                        clickedOnSelf = true;
                        break;
                    }
                }

                // 如果点击在 UI 上但不在本模块上，关闭
                if (!clickedOnSelf)
                {
                    CloseModule();
                }
            }
            else
            {
                // 点击在 UI 之外（3D场景空白处），也关闭
                CloseModule();
            }
        }
    }

    void CloseModule()
    {
        if (closeAll)
        {
            // 关闭所有大模块（通过管理器）
            if (BigModuleManager.Instance != null)
            {
                BigModuleManager.Instance.CloseAll();
            }
        }
        else
        {
            // 只关闭当前模块
            this.gameObject.SetActive(false);
        }
    }
}