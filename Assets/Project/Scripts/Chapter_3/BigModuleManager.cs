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

        foreach (var module in allModules)
        {
            if (module != null)
                module.SetActive(false);
        }
    }

    // 打开指定的大模块，自动关闭其他的
    public void OpenModule(GameObject module)
    {
        if (module == null) return;


        if (currentActiveModule == module && module.activeSelf)
        {

            return;
        }


        CloseAll();


        module.SetActive(true);
        currentActiveModule = module;
    }

    // 关闭所有大模块
    public void CloseAll()
    {
        foreach (var module in allModules)
        {
            if (module != null && module.activeSelf)
                module.SetActive(false);
        }
        currentActiveModule = null;
    }

    public void CloseCurrent()
    {
        if (currentActiveModule != null)
        {
            currentActiveModule.SetActive(false);
            currentActiveModule = null;
        }
    }

    public bool IsModuleOpen(GameObject module)
    {
        return currentActiveModule == module && module != null && module.activeSelf;
    }
}