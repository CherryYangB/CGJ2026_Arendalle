using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BigModuleManager : MonoBehaviour
{
    public static BigModuleManager Instance;

    [Header("所有的大模块（拖入）")]
    public List<GameObject> allModules = new List<GameObject>();  // 所有大模块列表

    private GameObject currentActiveModule = null;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        // 确保所有大模块默认都是关闭的
        foreach (var module in allModules)
        {
            if (module != null)
                module.SetActive(false);
        }
        currentActiveModule = null;
    }

    /// <summary>
    /// 打开指定的大模块，自动关闭其他的
    /// </summary>
    public void OpenModule(GameObject module)
    {
        if (module == null)
        {
            Debug.LogWarning("OpenModule: 传入的模块为 null");
            return;
        }

        // 【关键修复】如果点击的是当前已经打开的模块，不做任何操作
        // 这样不会干扰大模块内部的翻转（CardFlip）或其他交互
        if (currentActiveModule == module && module.activeSelf)
        {
            Debug.Log("OpenModule: 模块 " + module.name + " 已经打开，不执行任何操作");
            return;
        }

        // 关闭所有已打开的模块
        CloseAll();

        // 打开新的模块
        module.SetActive(true);
        currentActiveModule = module;
        Debug.Log("OpenModule: 打开模块 " + module.name);
    }

    /// <summary>
    /// 关闭所有大模块
    /// </summary>
    public void CloseAll()
    {
        foreach (var module in allModules)
        {
            if (module != null && module.activeSelf)
            {
                module.SetActive(false);
                Debug.Log("CloseAll: 关闭模块 " + module.name);
            }
        }
        currentActiveModule = null;
        Debug.Log("CloseAll: 所有模块已关闭");
    }

    /// <summary>
    /// 关闭当前激活的大模块
    /// </summary>
    public void CloseCurrent()
    {
        if (currentActiveModule != null)
        {
            Debug.Log("CloseCurrent: 关闭当前模块 " + currentActiveModule.name);
            currentActiveModule.SetActive(false);
            currentActiveModule = null;
        }
        else
        {
            Debug.Log("CloseCurrent: 没有激活的模块");
        }
    }

    /// <summary>
    /// 判断某个大模块是否正在打开
    /// </summary>
    public bool IsModuleOpen(GameObject module)
    {
        return currentActiveModule == module && module != null && module.activeSelf;
    }

    /// <summary>
    /// 获取当前激活的模块（外部可用）
    /// </summary>
    public GameObject GetCurrentActiveModule()
    {
        return currentActiveModule;
    }
}