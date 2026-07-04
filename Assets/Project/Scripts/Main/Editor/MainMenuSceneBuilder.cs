using System.Collections.Generic;
using System.IO;
using Arendalle;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Arendalle.EditorTools
{
    public static class MainMenuSceneBuilder
    {
        private const int ReferenceWidth = 1920;
        private const int ReferenceHeight = 1080;

        private const string HomeScenePath = "Assets/Project/Scenes/Home.unity";
        private const string ChapterScenePath = "Assets/Project/Scenes/Chapter_1.unity";
        private const string BorderSpritePath = "Assets/Project/Art/UI/HomeMenuBorder.png";
        private const string ButtonSpritePath = "Assets/Project/Art/UI/HandDrawnButton.png";
        private const string ChapterBackgroundSpritePath = "Assets/Project/Art/Background/bg_bg_s1_v1.png";
        private const string MemoHomeSpritePath = "Assets/Project/Art/Background/bg_memo_home.png";
        private const string MemoPageOneSpritePath = "Assets/Project/Art/Background/bg_memo_page_1.png";
        private const string TodoListSpritePath = "Assets/Project/Art/Sprites/todo_list.png";
        private const string BlueBarcodeCardSpritePath = "Assets/Project/Art/Sprites/blue_barcode_card.png";
        private const string YellowNoteClipSpritePath = "Assets/Project/Art/Sprites/yellow_note_clip.png";
        private const string AutoRunRequestPath = "Temp/RebuildHomeMenu.request";

        static MainMenuSceneBuilder()
        {
            EditorApplication.delayCall += RunPendingRebuildRequest;
        }

        [MenuItem("Arendalle/Rebuild Home Menu Scene")]
        public static void Rebuild()
        {
            EnsureFolders();
            CreateUiSprites();
            EnsureChapterSprites();
            BuildHomeScene();
            BuildChapterScene();
            UpdateBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Project scenes rebuilt.");
        }

        private static void RunPendingRebuildRequest()
        {
            string requestPath = GetProjectPath(AutoRunRequestPath);
            if (!File.Exists(requestPath))
            {
                return;
            }

            try
            {
                File.Delete(requestPath);
                Rebuild();
            }
            catch (System.Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/Project");
            EnsureFolder("Assets/Project/Scenes");
            EnsureFolder("Assets/Project/Art");
            EnsureFolder("Assets/Project/Art/UI");
            EnsureFolder("Assets/Project/Scripts");
            EnsureFolder("Assets/Project/Scripts/Editor");
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folder = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent))
            {
                EnsureFolder(parent);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static void CreateUiSprites()
        {
            CreateBorderSprite(BorderSpritePath, ReferenceWidth, ReferenceHeight);
            CreateButtonSprite(ButtonSpritePath, 560, 148);
        }

        private static void EnsureChapterSprites()
        {
            EnsureSpriteImport(ChapterBackgroundSpritePath);
            EnsureSpriteImport(MemoHomeSpritePath);
            EnsureSpriteImport(MemoPageOneSpritePath);
            EnsureSpriteImport(TodoListSpritePath);
            EnsureSpriteImport(BlueBarcodeCardSpritePath);
            EnsureSpriteImport(YellowNoteClipSpritePath);
        }

        private static void EnsureSpriteImport(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
            {
                Debug.LogWarning($"Sprite source not found: {assetPath}");
                return;
            }

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.mipmapEnabled)
            {
                importer.mipmapEnabled = false;
                changed = true;
            }

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, 100f))
            {
                importer.spritePixelsPerUnit = 100f;
                changed = true;
            }

            if (changed)
            {
                importer.SaveAndReimport();
            }
        }

        private static void CreateBorderSprite(string assetPath, int width, int height)
        {
            Texture2D texture = CreateTransparentTexture(width, height);
            Color32 ink = new Color32(12, 12, 12, 255);

            List<Vector2Int> points = new List<Vector2Int>();
            int left = 88;
            int right = width - 82;
            int bottom = 70;
            int top = height - 58;

            for (int x = left; x <= right; x += 20)
            {
                float t = (x - left) / (float)(right - left);
                points.Add(new Vector2Int(x, top + Mathf.RoundToInt(Mathf.Sin(t * 17f) * 7f + Mathf.Sin(t * 31f) * 4f)));
            }

            for (int y = top; y >= bottom; y -= 20)
            {
                float t = (top - y) / (float)(top - bottom);
                points.Add(new Vector2Int(right + Mathf.RoundToInt(Mathf.Sin(t * 14f + 1.4f) * 9f), y));
            }

            for (int x = right; x >= left; x -= 20)
            {
                float t = (right - x) / (float)(right - left);
                points.Add(new Vector2Int(x, bottom + Mathf.RoundToInt(Mathf.Sin(t * 19f + 0.7f) * 8f)));
            }

            for (int y = bottom; y <= top; y += 20)
            {
                float t = (y - bottom) / (float)(top - bottom);
                points.Add(new Vector2Int(left + Mathf.RoundToInt(Mathf.Sin(t * 16f + 2.2f) * 8f), y));
            }

            DrawPolyline(texture, points, ink, 7, true);
            SaveSprite(texture, assetPath, Vector4.zero);
        }

        private static void CreateButtonSprite(string assetPath, int width, int height)
        {
            Texture2D texture = CreateTransparentTexture(width, height);
            Color32 paper = new Color32(247, 244, 228, 210);
            Color32 ink = new Color32(12, 12, 12, 255);

            FillRect(texture, 18, 18, width - 36, height - 36, paper);

            List<Vector2Int> points = new List<Vector2Int>();
            int left = 24;
            int right = width - 26;
            int bottom = 24;
            int top = height - 22;

            for (int x = left; x <= right; x += 16)
            {
                float t = (x - left) / (float)(right - left);
                points.Add(new Vector2Int(x, top + Mathf.RoundToInt(Mathf.Sin(t * 18f) * 4f)));
            }

            for (int y = top; y >= bottom; y -= 16)
            {
                float t = (top - y) / (float)(top - bottom);
                points.Add(new Vector2Int(right + Mathf.RoundToInt(Mathf.Sin(t * 12f + 0.9f) * 4f), y));
            }

            for (int x = right; x >= left; x -= 16)
            {
                float t = (right - x) / (float)(right - left);
                points.Add(new Vector2Int(x, bottom + Mathf.RoundToInt(Mathf.Sin(t * 15f + 1.6f) * 4f)));
            }

            for (int y = bottom; y <= top; y += 16)
            {
                float t = (y - bottom) / (float)(top - bottom);
                points.Add(new Vector2Int(left + Mathf.RoundToInt(Mathf.Sin(t * 13f + 2.1f) * 4f), y));
            }

            DrawPolyline(texture, points, ink, 5, true);
            SaveSprite(texture, assetPath, new Vector4(45, 45, 45, 45));
        }

        private static Texture2D CreateTransparentTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color32[] pixels = new Color32[width * height];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(0, 0, 0, 0);
            }

            texture.SetPixels32(pixels);
            return texture;
        }

        private static void FillRect(Texture2D texture, int x, int y, int width, int height, Color32 color)
        {
            for (int py = y; py < y + height; py++)
            {
                for (int px = x; px < x + width; px++)
                {
                    SetPixel(texture, px, py, color);
                }
            }
        }

        private static void DrawPolyline(Texture2D texture, IReadOnlyList<Vector2Int> points, Color32 color, int width, bool close)
        {
            for (int i = 0; i < points.Count - 1; i++)
            {
                DrawLine(texture, points[i], points[i + 1], color, width);
            }

            if (close && points.Count > 1)
            {
                DrawLine(texture, points[points.Count - 1], points[0], color, width);
            }

            texture.Apply(false, false);
        }

        private static void DrawLine(Texture2D texture, Vector2Int start, Vector2Int end, Color32 color, int width)
        {
            int dx = Mathf.Abs(end.x - start.x);
            int dy = Mathf.Abs(end.y - start.y);
            int sx = start.x < end.x ? 1 : -1;
            int sy = start.y < end.y ? 1 : -1;
            int err = dx - dy;
            int x = start.x;
            int y = start.y;

            while (true)
            {
                DrawBrush(texture, x, y, width, color);
                if (x == end.x && y == end.y)
                {
                    break;
                }

                int e2 = err * 2;
                if (e2 > -dy)
                {
                    err -= dy;
                    x += sx;
                }

                if (e2 < dx)
                {
                    err += dx;
                    y += sy;
                }
            }
        }

        private static void DrawBrush(Texture2D texture, int centerX, int centerY, int width, Color32 color)
        {
            int radius = Mathf.Max(1, width / 2);
            for (int y = centerY - radius; y <= centerY + radius; y++)
            {
                for (int x = centerX - radius; x <= centerX + radius; x++)
                {
                    if ((x - centerX) * (x - centerX) + (y - centerY) * (y - centerY) <= radius * radius)
                    {
                        SetPixel(texture, x, y, color);
                    }
                }
            }
        }

        private static void SetPixel(Texture2D texture, int x, int y, Color32 color)
        {
            if (x < 0 || y < 0 || x >= texture.width || y >= texture.height)
            {
                return;
            }

            texture.SetPixel(x, y, color);
        }

        private static void SaveSprite(Texture2D texture, string assetPath, Vector4 border)
        {
            string fullPath = GetProjectPath(assetPath);
            string directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(fullPath, texture.EncodeToPNG());
            Object.DestroyImmediate(texture);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            TextureImporter importer = (TextureImporter)AssetImporter.GetAtPath(assetPath);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.spritePixelsPerUnit = 100f;
            importer.spriteBorder = border;
            importer.SaveAndReimport();
        }

        private static string GetProjectPath(string relativePath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", relativePath));
        }

        private static void BuildHomeScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(new Color(0.93f, 0.92f, 0.86f, 1f));
            CreateEventSystem();

            Sprite borderSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BorderSpritePath);
            Sprite buttonSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ButtonSpritePath);
            Font font = GetDefaultFont();

            GameObject canvasObject = new GameObject("HomeMenuCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            GameObject homeRoot = CreateUiObject("HomeRoot", canvasObject.transform);
            Stretch(homeRoot.GetComponent<RectTransform>());
            CanvasGroup homeGroup = homeRoot.AddComponent<CanvasGroup>();
            homeGroup.alpha = 1f;
            homeGroup.interactable = true;
            homeGroup.blocksRaycasts = true;

            Image background = CreateImage("ParchmentBackground", homeRoot.transform, null, new Color(0.93f, 0.92f, 0.86f, 1f));
            Stretch(background.rectTransform);

            Image border = CreateImage("HandDrawnBorder", homeRoot.transform, borderSprite, Color.white);
            Stretch(border.rectTransform);
            border.raycastTarget = false;

            Text title = CreateText("Title", homeRoot.transform, "Background", font, 92, TextAnchor.MiddleCenter);
            title.color = new Color(0.02f, 0.02f, 0.02f, 1f);
            SetRect(title.rectTransform, new Vector2(0.68f, 0.59f), new Vector2(710f, 170f));

            Button startButton = CreateButton("StartButton", homeRoot.transform, "开始", buttonSprite, font, new Vector2(0.29f, 0.47f));
            Button quitButton = CreateButton("QuitButton", homeRoot.transform, "退出游戏", buttonSprite, font, new Vector2(0.29f, 0.32f));
            Button aboutButton = CreateButton("AboutButton", homeRoot.transform, "关于", buttonSprite, font, new Vector2(0.29f, 0.18f));

            GameObject aboutRoot = CreateUiObject("AboutRoot", canvasObject.transform);
            Stretch(aboutRoot.GetComponent<RectTransform>());
            CanvasGroup aboutGroup = aboutRoot.AddComponent<CanvasGroup>();
            aboutGroup.alpha = 0f;
            aboutGroup.interactable = false;
            aboutGroup.blocksRaycasts = false;

            Text aboutText = CreateText(
                "AboutText",
                aboutRoot.transform,
                "关于\n\n这里是文本信息占位。\n可以替换为制作组、剧情简介或操作说明。\n\n感谢游玩。",
                font,
                48,
                TextAnchor.MiddleCenter);
            aboutText.color = new Color(0.02f, 0.02f, 0.02f, 1f);
            aboutText.lineSpacing = 1.12f;
            SetRect(aboutText.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(1180f, 560f));

            Image whiteFade = CreateImage("WhiteFade", canvasObject.transform, null, Color.white);
            Stretch(whiteFade.rectTransform);
            Color fadeColor = whiteFade.color;
            fadeColor.a = 0f;
            whiteFade.color = fadeColor;
            whiteFade.raycastTarget = false;

            GameObject musicObject = new GameObject("MusicLoopPlaceholder", typeof(AudioSource));
            AudioSource musicSource = musicObject.GetComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.playOnAwake = true;
            musicSource.volume = 0.55f;

            GameObject controllerObject = new GameObject("MainMenuController");
            MainMenuController controller = controllerObject.AddComponent<MainMenuController>();
            ConfigureController(controller, startButton, quitButton, aboutButton, homeGroup, aboutGroup, whiteFade, musicSource);

            EditorSceneManager.SaveScene(scene, HomeScenePath);
        }

        private static void BuildChapterScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateCamera(Color.white);
            CreateEventSystem();

            Sprite backgroundSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ChapterBackgroundSpritePath);
            Sprite memoHomeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MemoHomeSpritePath);
            Sprite memoPageOneSprite = AssetDatabase.LoadAssetAtPath<Sprite>(MemoPageOneSpritePath);
            Sprite todoListSprite = AssetDatabase.LoadAssetAtPath<Sprite>(TodoListSpritePath);
            Sprite blueBarcodeCardSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BlueBarcodeCardSpritePath);
            Sprite yellowNoteClipSprite = AssetDatabase.LoadAssetAtPath<Sprite>(YellowNoteClipSpritePath);
            Font font = GetDefaultFont();

            GameObject canvasObject = new GameObject("ChapterCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            Image background = CreateImage("Background", canvasObject.transform, backgroundSprite, Color.white);
            Stretch(background.rectTransform);
            background.raycastTarget = false;

            GameObject memoRoot = CreateUiObject("MemoRoot", canvasObject.transform);
            SetRect(memoRoot.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(1672f, 941f));

            Image memoHome = CreateImage("MemoHome", memoRoot.transform, memoHomeSprite, Color.white);
            Stretch(memoHome.rectTransform);
            memoHome.raycastTarget = false;

            Image incomingPage = CreateImage("IncomingPage", memoRoot.transform, memoPageOneSprite, new Color(1f, 1f, 1f, 0f));
            Stretch(incomingPage.rectTransform);
            incomingPage.raycastTarget = false;
            incomingPage.gameObject.SetActive(false);

            GameObject itemRoot = CreateUiObject("MemoItems", memoRoot.transform);
            Stretch(itemRoot.GetComponent<RectTransform>());
            CanvasGroup itemGroup = itemRoot.AddComponent<CanvasGroup>();
            itemGroup.alpha = 1f;
            itemGroup.interactable = true;
            itemGroup.blocksRaycasts = true;

            Button todoListButton = CreateClickableImage(
                "TodoList",
                itemRoot.transform,
                todoListSprite,
                new Vector2(-508f, 300f),
                new Vector2(390f, 246f));

            Button blueBarcodeCardButton = CreateClickableImage(
                "BlueBarcodeCard",
                itemRoot.transform,
                blueBarcodeCardSprite,
                new Vector2(-520f, 75f),
                new Vector2(350f, 263f));

            Button yellowNoteClipButton = CreateClickableImage(
                "YellowNoteClip",
                itemRoot.transform,
                yellowNoteClipSprite,
                new Vector2(-490f, -185f),
                new Vector2(392f, 377f));

            Image pageTurnHighlight = CreateImage("PageTurnHighlight", memoRoot.transform, null, new Color(1f, 1f, 1f, 0f));
            SetRect(pageTurnHighlight.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(64f, 820f), new Vector2(575f, 0f));
            pageTurnHighlight.raycastTarget = false;
            pageTurnHighlight.gameObject.SetActive(false);

            Image pageEdgeHotspotImage = CreateImage("PageEdgeHotspot", memoRoot.transform, null, new Color(1f, 1f, 1f, 0f));
            SetRect(pageEdgeHotspotImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(170f, 760f), new Vector2(575f, 0f));
            pageEdgeHotspotImage.raycastTarget = true;

            Button pageEdgeButton = AddInvisibleButton(pageEdgeHotspotImage);

            Image previousPageEdgeHotspotImage = CreateImage("PreviousPageEdgeHotspot", memoRoot.transform, null, new Color(1f, 1f, 1f, 0f));
            SetRect(previousPageEdgeHotspotImage.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(170f, 760f), new Vector2(-575f, 0f));
            previousPageEdgeHotspotImage.raycastTarget = true;

            Button previousPageEdgeButton = AddInvisibleButton(previousPageEdgeHotspotImage);

            GameObject detailRoot = CreateUiObject("DetailOverlay", canvasObject.transform);
            Stretch(detailRoot.GetComponent<RectTransform>());
            CanvasGroup detailGroup = detailRoot.AddComponent<CanvasGroup>();
            detailGroup.alpha = 0f;
            detailGroup.interactable = false;
            detailGroup.blocksRaycasts = false;

            Image detailBackdrop = CreateImage("DetailBackdrop", detailRoot.transform, null, new Color(0f, 0f, 0f, 0f));
            Stretch(detailBackdrop.rectTransform);
            detailBackdrop.raycastTarget = true;
            Button detailBackdropButton = AddInvisibleButton(detailBackdrop);

            Image detailObject = CreateImage("DetailObject", detailRoot.transform, null, Color.white);
            SetRect(detailObject.rectTransform, new Vector2(0.5f, 0.58f), new Vector2(640f, 520f));
            detailObject.preserveAspect = true;
            detailObject.raycastTarget = false;

            Text detailText = CreateText("DetailText", detailRoot.transform, string.Empty, font, 42, TextAnchor.MiddleCenter);
            detailText.color = new Color(1f, 0.97f, 0.88f, 0f);
            detailText.lineSpacing = 1.12f;
            SetRect(detailText.rectTransform, new Vector2(0.5f, 0.19f), new Vector2(980f, 190f));
            detailRoot.SetActive(false);

            GameObject controllerObject = new GameObject("ChapterOneController");
            ChapterOneController controller = controllerObject.AddComponent<ChapterOneController>();
            ConfigureChapterOneController(
                controller,
                memoHome,
                memoHomeSprite,
                memoPageOneSprite,
                incomingPage,
                pageTurnHighlight,
                pageEdgeButton,
                previousPageEdgeButton,
                itemGroup,
                todoListButton,
                blueBarcodeCardButton,
                yellowNoteClipButton,
                todoListSprite,
                blueBarcodeCardSprite,
                yellowNoteClipSprite,
                detailGroup,
                detailBackdropButton,
                detailBackdrop,
                detailObject,
                detailText);

            EditorSceneManager.SaveScene(scene, ChapterScenePath);
        }

        private static void CreateCamera(Color backgroundColor)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            cameraObject.transform.position = new Vector3(0f, 0f, -10f);

            Camera camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = backgroundColor;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
        }

        private static void CreateEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null)
            {
                return;
            }

            new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        private static Image CreateImage(string name, Transform parent, Sprite sprite, Color color)
        {
            GameObject gameObject = CreateUiObject(name, parent);
            Image image = gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = sprite != null && sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
            return image;
        }

        private static Text CreateText(string name, Transform parent, string value, Font font, int size, TextAnchor anchor)
        {
            GameObject gameObject = CreateUiObject(name, parent);
            Text text = gameObject.AddComponent<Text>();
            text.text = value;
            text.font = font;
            text.fontSize = size;
            text.alignment = anchor;
            text.horizontalOverflow = HorizontalWrapMode.Wrap;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateButton(string name, Transform parent, string label, Sprite sprite, Font font, Vector2 anchor)
        {
            Image image = CreateImage(name, parent, sprite, Color.white);
            RectTransform rectTransform = image.rectTransform;
            SetRect(rectTransform, anchor, new Vector2(500f, 120f));
            image.raycastTarget = true;

            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.96f, 0.94f, 0.86f, 1f);
            colors.pressedColor = new Color(0.88f, 0.86f, 0.78f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(1f, 1f, 1f, 0.45f);
            colors.fadeDuration = 0.12f;
            button.colors = colors;

            Text text = CreateText("Label", image.transform, label, font, 54, TextAnchor.MiddleCenter);
            text.color = new Color(0.02f, 0.02f, 0.02f, 1f);
            Stretch(text.rectTransform);

            return button;
        }

        private static Button CreateClickableImage(string name, Transform parent, Sprite sprite, Vector2 anchoredPosition, Vector2 size)
        {
            Image image = CreateImage(name, parent, sprite, Color.white);
            SetRect(image.rectTransform, new Vector2(0.5f, 0.5f), size, anchoredPosition);
            image.preserveAspect = true;
            image.raycastTarget = true;

            return AddInvisibleButton(image);
        }

        private static Button AddInvisibleButton(Image image)
        {
            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.None;
            return button;
        }

        private static GameObject CreateUiObject(string name, Transform parent)
        {
            GameObject gameObject = new GameObject(name, typeof(RectTransform));
            gameObject.transform.SetParent(parent, false);
            return gameObject;
        }

        private static void Stretch(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.localScale = Vector3.one;
        }

        private static void SetRect(RectTransform rectTransform, Vector2 normalizedAnchor, Vector2 size)
        {
            SetRect(rectTransform, normalizedAnchor, size, Vector2.zero);
        }

        private static void SetRect(RectTransform rectTransform, Vector2 normalizedAnchor, Vector2 size, Vector2 anchoredPosition)
        {
            rectTransform.anchorMin = normalizedAnchor;
            rectTransform.anchorMax = normalizedAnchor;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.localScale = Vector3.one;
        }

        private static Font GetDefaultFont()
        {
            Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font != null)
            {
                return font;
            }

            return Font.CreateDynamicFontFromOSFont(
                new[] { "PingFang SC", "Hiragino Sans GB", "Microsoft YaHei", "Arial Unicode MS", "Arial" },
                32);
        }

        private static void ConfigureController(
            MainMenuController controller,
            Button startButton,
            Button quitButton,
            Button aboutButton,
            CanvasGroup homeGroup,
            CanvasGroup aboutGroup,
            Image whiteFade,
            AudioSource musicSource)
        {
            SerializedObject serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("chapterSceneName").stringValue = "Chapter_1";
            serializedObject.FindProperty("startButton").objectReferenceValue = startButton;
            serializedObject.FindProperty("quitButton").objectReferenceValue = quitButton;
            serializedObject.FindProperty("aboutButton").objectReferenceValue = aboutButton;
            serializedObject.FindProperty("homeGroup").objectReferenceValue = homeGroup;
            serializedObject.FindProperty("aboutGroup").objectReferenceValue = aboutGroup;
            serializedObject.FindProperty("whiteFade").objectReferenceValue = whiteFade;
            serializedObject.FindProperty("sceneFadeDuration").floatValue = 1.15f;
            serializedObject.FindProperty("aboutFadeDuration").floatValue = 0.9f;
            serializedObject.FindProperty("musicSource").objectReferenceValue = musicSource;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureChapterOneController(
            ChapterOneController controller,
            Image memoImage,
            Sprite memoHomeSprite,
            Sprite memoPageOneSprite,
            Image incomingPageImage,
            Image pageTurnHighlight,
            Button pageEdgeButton,
            Button previousPageEdgeButton,
            CanvasGroup itemGroup,
            Button todoListButton,
            Button blueBarcodeCardButton,
            Button yellowNoteClipButton,
            Sprite todoListDetailSprite,
            Sprite blueBarcodeCardDetailSprite,
            Sprite yellowNoteClipDetailSprite,
            CanvasGroup detailGroup,
            Button detailBackdropButton,
            Image detailBackdropImage,
            Image detailObjectImage,
            Text detailText)
        {
            SerializedObject serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("memoImage").objectReferenceValue = memoImage;
            serializedObject.FindProperty("memoHomeSprite").objectReferenceValue = memoHomeSprite;
            serializedObject.FindProperty("memoPageOneSprite").objectReferenceValue = memoPageOneSprite;
            serializedObject.FindProperty("incomingPageImage").objectReferenceValue = incomingPageImage;
            serializedObject.FindProperty("pageTurnHighlight").objectReferenceValue = pageTurnHighlight;
            serializedObject.FindProperty("pageEdgeButton").objectReferenceValue = pageEdgeButton;
            serializedObject.FindProperty("previousPageEdgeButton").objectReferenceValue = previousPageEdgeButton;
            serializedObject.FindProperty("itemGroup").objectReferenceValue = itemGroup;
            serializedObject.FindProperty("todoListButton").objectReferenceValue = todoListButton;
            serializedObject.FindProperty("todoListTransform").objectReferenceValue = todoListButton.GetComponent<RectTransform>();
            serializedObject.FindProperty("todoListDetailSprite").objectReferenceValue = todoListDetailSprite;
            serializedObject.FindProperty("todoListDetailText").stringValue = "待办清单\n还没完成的事项被匆忙写下，像是在提醒某个必须按时完成的约定。";
            serializedObject.FindProperty("blueBarcodeCardButton").objectReferenceValue = blueBarcodeCardButton;
            serializedObject.FindProperty("blueBarcodeCardTransform").objectReferenceValue = blueBarcodeCardButton.GetComponent<RectTransform>();
            serializedObject.FindProperty("blueBarcodeCardDetailSprite").objectReferenceValue = blueBarcodeCardDetailSprite;
            serializedObject.FindProperty("blueBarcodeCardDetailText").stringValue = "蓝色条码卡\n折角的卡片保留着一串条码，也许能对应某处锁住的入口。";
            serializedObject.FindProperty("yellowNoteClipButton").objectReferenceValue = yellowNoteClipButton;
            serializedObject.FindProperty("yellowNoteClipTransform").objectReferenceValue = yellowNoteClipButton.GetComponent<RectTransform>();
            serializedObject.FindProperty("yellowNoteClipDetailSprite").objectReferenceValue = yellowNoteClipDetailSprite;
            serializedObject.FindProperty("yellowNoteClipDetailText").stringValue = "黄色便签\n纸上写着按时吃药，边角的磨损说明它已经被反复看过很多次。";
            serializedObject.FindProperty("detailGroup").objectReferenceValue = detailGroup;
            serializedObject.FindProperty("detailBackdropButton").objectReferenceValue = detailBackdropButton;
            serializedObject.FindProperty("detailBackdropImage").objectReferenceValue = detailBackdropImage;
            serializedObject.FindProperty("detailObjectImage").objectReferenceValue = detailObjectImage;
            serializedObject.FindProperty("detailText").objectReferenceValue = detailText;
            serializedObject.FindProperty("detailFadeDuration").floatValue = 0.24f;
            serializedObject.FindProperty("detailScaleDuration").floatValue = 0.28f;
            serializedObject.FindProperty("detailBackdropAlpha").floatValue = 0.68f;
            serializedObject.FindProperty("pageTurnDuration").floatValue = 0.68f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void UpdateBuildSettings()
        {
            string[] desiredScenes =
            {
                HomeScenePath,
                ChapterScenePath,
                "Assets/Scenes/SampleScene.unity"
            };

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (string scenePath in desiredScenes)
            {
                if (File.Exists(scenePath))
                {
                    scenes.Add(new EditorBuildSettingsScene(scenePath, true));
                }
            }

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
