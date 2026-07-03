#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class HomeSceneBuilder
{
    private const string SceneFolder = "Assets/Project/Scenes";
    private const string HomeScenePath = SceneFolder + "/Home.unity";
    private static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);
    private static readonly Vector2 ButtonSize = new Vector2(560f, 96f);

    private static readonly MenuButtonData[] MenuButtons =
    {
        new("Star", "Star", new Vector2(430f, -350f)),
        new("Local", "Local", new Vector2(430f, -475f)),
        new("CG", "CG", new Vector2(430f, -600f)),
        new("About", "About", new Vector2(430f, -725f)),
    };

    [MenuItem("CGJ2026/Build Home Scene")]
    public static void Build()
    {
        Directory.CreateDirectory(SceneFolder);

        CreateHomeScene();
        CreatePlaceholderScenes();
        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Home scene and placeholder target scenes were generated.");
    }

    private static void CreateHomeScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        scene.name = "Home";

        GameObject cameraObject = new GameObject("Main Camera");
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.93f, 0.92f, 0.86f, 1f);
        camera.orthographic = true;
        cameraObject.AddComponent<AudioListener>();
        cameraObject.tag = "MainCamera";

        GameObject eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<StandaloneInputModule>();

        GameObject canvasObject = new GameObject("HomeCanvas");
        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = ReferenceResolution;
        scaler.matchWidthOrHeight = 0.5f;
        canvasObject.AddComponent<GraphicRaycaster>();

        MainMenuSceneLoader loader = canvasObject.AddComponent<MainMenuSceneLoader>();

        RectTransform canvasRect = canvasObject.GetComponent<RectTransform>();
        RectTransform safeFrameRect = CreateRectTransform("SafeFrame16x9", canvasRect);
        ConfigureRect(safeFrameRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, ReferenceResolution);

        Image background = CreateImage("BackgroundImage", safeFrameRect, new Color(0.88f, 0.87f, 0.80f, 1f));
        StretchToParent(background.rectTransform);

        Image nameImage = CreateImage("NameImage", safeFrameRect, new Color(1f, 1f, 1f, 0.55f));
        ConfigureRect(nameImage.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(490f, -205f), new Vector2(700f, 245f));

        AddPlaceholderLabel(nameImage.rectTransform, "NameImage_Label", "Name", Vector2.zero, Vector2.zero, 72f, true);
        AddBorder(nameImage.rectTransform, "NameImage_Border");

        foreach (MenuButtonData data in MenuButtons)
        {
            CreateMenuButton(safeFrameRect, loader, data);
        }

        AddPlaceholderLabel(background.rectTransform, "BackgroundImage_Label", "Background", new Vector2(320f, 0f), new Vector2(760f, 220f), 96f);

        EditorSceneManager.SaveScene(scene, HomeScenePath);
    }

    private static void CreatePlaceholderScenes()
    {
        foreach (MenuButtonData data in MenuButtons)
        {
            string path = $"{SceneFolder}/{data.SceneName}.unity";
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            scene.name = data.SceneName;

            GameObject cameraObject = new GameObject("Main Camera");
            Camera camera = cameraObject.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.12f, 0.13f, 0.16f, 1f);
            camera.orthographic = true;
            cameraObject.AddComponent<AudioListener>();
            cameraObject.tag = "MainCamera";

            GameObject canvasObject = new GameObject(data.SceneName + "Canvas");
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform safeFrameRect = CreateRectTransform("SafeFrame16x9", canvasObject.GetComponent<RectTransform>());
            ConfigureRect(safeFrameRect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, ReferenceResolution);

            AddPlaceholderLabel(safeFrameRect, data.SceneName + "_Label", data.SceneName, Vector2.zero, new Vector2(700f, 180f), 96f);

            EditorSceneManager.SaveScene(scene, path);
        }
    }

    private static void UpdateBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>
        {
            new(HomeScenePath, true)
        };

        foreach (MenuButtonData data in MenuButtons)
        {
            scenes.Add(new EditorBuildSettingsScene($"{SceneFolder}/{data.SceneName}.unity", true));
        }

        HashSet<string> generatedScenePaths = new HashSet<string>();
        foreach (EditorBuildSettingsScene scene in scenes)
        {
            generatedScenePaths.Add(scene.path);
        }

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (!generatedScenePaths.Contains(scene.path))
            {
                scenes.Add(scene);
            }
        }

        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static Image CreateImage(string name, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(name);
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static RectTransform CreateRectTransform(string name, Transform parent)
    {
        GameObject rectObject = new GameObject(name);
        rectObject.transform.SetParent(parent, false);
        return rectObject.AddComponent<RectTransform>();
    }

    private static Button CreateMenuButton(RectTransform parent, MainMenuSceneLoader loader, MenuButtonData data)
    {
        GameObject buttonObject = new GameObject(data.ButtonName + "Button");
        buttonObject.transform.SetParent(parent, false);

        RectTransform rect = buttonObject.AddComponent<RectTransform>();
        ConfigureRect(rect, new Vector2(0f, 1f), new Vector2(0f, 1f), data.AnchoredPosition, ButtonSize);

        Image image = buttonObject.AddComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.72f);

        Button button = buttonObject.AddComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(0.95f, 0.95f, 0.95f, 1f);
        colors.pressedColor = new Color(0.78f, 0.78f, 0.78f, 1f);
        button.colors = colors;
        UnityEventTools.AddStringPersistentListener(button.onClick, loader.LoadScene, data.SceneName);

        AddBorder(rect, data.ButtonName + "Button_Border");
        AddButtonLabel(rect, data.ButtonName + "ButtonLabel", data.ButtonName);

        return button;
    }

    private static void AddButtonLabel(RectTransform parent, string name, string text)
    {
        GameObject labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, false);

        Text label = labelObject.AddComponent<Text>();
        label.text = text;
        label.alignment = TextAnchor.MiddleCenter;
        label.font = GetDefaultFont();
        label.fontSize = 56;
        label.color = Color.black;
        label.raycastTarget = false;

        StretchToParent(label.rectTransform);
    }

    private static void AddPlaceholderLabel(RectTransform parent, string name, string text, Vector2 anchoredPosition, Vector2 size, float fontSize)
    {
        AddPlaceholderLabel(parent, name, text, anchoredPosition, size, fontSize, false);
    }

    private static void AddPlaceholderLabel(RectTransform parent, string name, string text, Vector2 anchoredPosition, Vector2 size, float fontSize, bool stretch)
    {
        GameObject labelObject = new GameObject(name);
        labelObject.transform.SetParent(parent, false);

        Text label = labelObject.AddComponent<Text>();
        label.text = text;
        label.alignment = TextAnchor.MiddleCenter;
        label.font = GetDefaultFont();
        label.fontSize = Mathf.RoundToInt(fontSize);
        label.color = Color.black;
        label.raycastTarget = false;

        RectTransform rect = label.rectTransform;
        if (stretch)
        {
            StretchToParent(rect);
        }
        else
        {
            ConfigureRect(rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), anchoredPosition, size);
        }
    }

    private static void AddBorder(RectTransform target, string name)
    {
        GameObject borderObject = new GameObject(name);
        borderObject.transform.SetParent(target, false);

        Image borderImage = borderObject.AddComponent<Image>();
        borderImage.color = new Color(1f, 1f, 1f, 0.01f);
        borderImage.raycastTarget = false;

        Outline outline = borderObject.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(4f, -4f);

        StretchToParent(borderImage.rectTransform);
    }

    private static void StretchToParent(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void ConfigureRect(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    private static Font GetDefaultFont()
    {
        return Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private readonly struct MenuButtonData
    {
        public MenuButtonData(string buttonName, string sceneName, Vector2 anchoredPosition)
        {
            ButtonName = buttonName;
            SceneName = sceneName;
            AnchoredPosition = anchoredPosition;
        }

        public string ButtonName { get; }
        public string SceneName { get; }
        public Vector2 AnchoredPosition { get; }
    }
}
#endif
