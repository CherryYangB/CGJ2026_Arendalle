using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Arendalle
{
    public sealed class ChapterOneController : MonoBehaviour
    {
        [Header("Memo")]
        [SerializeField] private Image memoImage;
        [SerializeField] private Sprite memoHomeSprite;
        [SerializeField] private Sprite memoPageOneSprite;
        [SerializeField] private Image incomingPageImage;
        [SerializeField] private Image pageTurnHighlight;
        [SerializeField] private Button pageEdgeButton;
        [SerializeField] private Button previousPageEdgeButton;

        [Header("Memo Items")]
        [SerializeField] private CanvasGroup itemGroup;
        [SerializeField] private Button todoListButton;
        [SerializeField] private RectTransform todoListTransform;
        [SerializeField] private Sprite todoListDetailSprite;
        [SerializeField] private string todoListDetailText;
        [SerializeField] private Button blueBarcodeCardButton;
        [SerializeField] private RectTransform blueBarcodeCardTransform;
        [SerializeField] private Sprite blueBarcodeCardDetailSprite;
        [SerializeField] private string blueBarcodeCardDetailText;
        [SerializeField] private Button yellowNoteClipButton;
        [SerializeField] private RectTransform yellowNoteClipTransform;
        [SerializeField] private Sprite yellowNoteClipDetailSprite;
        [SerializeField] private string yellowNoteClipDetailText;

        [Header("Watch")]
        [SerializeField] private WatchTimeDisplay watchTimeDisplay;

        [Header("Detail Overlay")]
        [SerializeField] private CanvasGroup detailGroup;
        [SerializeField] private Button detailBackdropButton;
        [SerializeField] private Image detailBackdropImage;
        [SerializeField] private Image detailObjectImage;
        [SerializeField] private Button detailObjectButton;
        [SerializeField] private Text detailText;

        [Header("Animation")]
        [SerializeField] private float detailFadeDuration = 0.24f;
        [SerializeField] private float detailScaleDuration = 0.28f;
        [SerializeField] private float detailBackdropAlpha = 0.68f;
        [SerializeField] private float pageTurnDuration = 0.68f;

        private Coroutine detailRoutine;
        private Coroutine pageTurnRoutine;
        private ChapterOnePageItem activePageItem;
        private Vector2 defaultDetailObjectSize;
        private Vector3 defaultDetailObjectEulerAngles;
        private bool detailOpen;
        private bool pageTurned;

        public bool IsPageTurned => pageTurned;
        public event Action<bool> PageTurnStateChanged;

        private void Awake()
        {
            CacheDetailObjectDefaults();
            ConfigureDetailObjectButton();

            if (memoImage != null && memoHomeSprite != null)
            {
                memoImage.sprite = memoHomeSprite;
            }

            SetImageAlpha(incomingPageImage, 0f);
            SetImageAlpha(pageTurnHighlight, 0f);
            SetDetailVisible(false, 0f);

            if (incomingPageImage != null)
            {
                incomingPageImage.gameObject.SetActive(false);
            }

            if (pageTurnHighlight != null)
            {
                pageTurnHighlight.gameObject.SetActive(false);
            }

            if (itemGroup != null)
            {
                itemGroup.alpha = 1f;
                itemGroup.interactable = true;
                itemGroup.blocksRaycasts = true;
            }

            RegisterListeners();
            RegisterWatchListeners();
            SetPageEdgeButtons(CanTurnForwardPage(), false);
        }

        private void OnDestroy()
        {
            if (todoListButton != null)
            {
                todoListButton.onClick.RemoveListener(OpenTodoListDetail);
            }

            if (blueBarcodeCardButton != null)
            {
                blueBarcodeCardButton.onClick.RemoveListener(OpenBlueBarcodeCardDetail);
            }

            if (yellowNoteClipButton != null)
            {
                yellowNoteClipButton.onClick.RemoveListener(OpenYellowNoteClipDetail);
            }

            if (detailBackdropButton != null)
            {
                detailBackdropButton.onClick.RemoveListener(CloseDetail);
            }

            if (detailObjectButton != null)
            {
                detailObjectButton.onClick.RemoveListener(ToggleActivePageItemSide);
            }

            if (pageEdgeButton != null)
            {
                pageEdgeButton.onClick.RemoveListener(TurnPage);
            }

            if (previousPageEdgeButton != null)
            {
                previousPageEdgeButton.onClick.RemoveListener(TurnBackPage);
            }

            if (watchTimeDisplay != null)
            {
                watchTimeDisplay.FirstOpened -= HandleWatchFirstOpened;
            }
        }

        private void RegisterListeners()
        {
            if (todoListButton != null)
            {
                todoListButton.onClick.AddListener(OpenTodoListDetail);
            }

            if (blueBarcodeCardButton != null)
            {
                blueBarcodeCardButton.onClick.AddListener(OpenBlueBarcodeCardDetail);
            }

            if (yellowNoteClipButton != null)
            {
                yellowNoteClipButton.onClick.AddListener(OpenYellowNoteClipDetail);
            }

            if (detailBackdropButton != null)
            {
                detailBackdropButton.onClick.AddListener(CloseDetail);
            }

            if (detailObjectButton != null)
            {
                detailObjectButton.onClick.RemoveListener(ToggleActivePageItemSide);
                detailObjectButton.onClick.AddListener(ToggleActivePageItemSide);
            }

            if (pageEdgeButton != null)
            {
                pageEdgeButton.onClick.AddListener(TurnPage);
            }

            if (previousPageEdgeButton != null)
            {
                previousPageEdgeButton.onClick.AddListener(TurnBackPage);
            }
        }

        private void RegisterWatchListeners()
        {
            if (watchTimeDisplay == null)
            {
                return;
            }

            watchTimeDisplay.FirstOpened -= HandleWatchFirstOpened;
            watchTimeDisplay.FirstOpened += HandleWatchFirstOpened;
        }

        private void OpenTodoListDetail()
        {
            OpenDetail(todoListDetailSprite, todoListDetailText);
        }

        private void OpenBlueBarcodeCardDetail()
        {
            OpenDetail(blueBarcodeCardDetailSprite, blueBarcodeCardDetailText);
        }

        private void OpenYellowNoteClipDetail()
        {
            OpenDetail(yellowNoteClipDetailSprite, yellowNoteClipDetailText);
        }

        private void OpenDetail(Sprite sprite, string text)
        {
            if (pageTurned || pageTurnRoutine != null)
            {
                return;
            }

            activePageItem = null;
            ApplyDetailContent(sprite, text, defaultDetailObjectSize, defaultDetailObjectEulerAngles, false);

            StartDetailRoutine(ShowDetailRoutine());
        }

        public void OpenPageItemDetail(ChapterOnePageItem item)
        {
            if (!pageTurned || detailOpen || pageTurnRoutine != null || IsWatchDetailOpen() || item == null)
            {
                return;
            }

            activePageItem = item;
            activePageItem.ResetDetailSide();
            ApplyActivePageItemDetail();
            StartDetailRoutine(ShowDetailRoutine());
        }

        public void CloseDetail()
        {
            if (!detailOpen)
            {
                return;
            }

            StartDetailRoutine(HideDetailRoutine());
        }

        public void TurnPage()
        {
            if (detailOpen || pageTurned || pageTurnRoutine != null || IsWatchDetailOpen() || !HasWatchBeenOpened())
            {
                return;
            }

            pageTurnRoutine = StartCoroutine(TurnPageRoutine());
        }

        public void TurnBackPage()
        {
            if (detailOpen || !pageTurned || pageTurnRoutine != null || IsWatchDetailOpen())
            {
                return;
            }

            pageTurnRoutine = StartCoroutine(TurnBackPageRoutine());
        }

        private void StartDetailRoutine(IEnumerator routine)
        {
            if (detailRoutine != null)
            {
                StopCoroutine(detailRoutine);
            }

            detailRoutine = StartCoroutine(routine);
        }

        private IEnumerator ShowDetailRoutine()
        {
            detailOpen = true;
            SetDetailVisible(true, 0f);

            if (itemGroup != null)
            {
                itemGroup.interactable = false;
                itemGroup.blocksRaycasts = false;
            }

            if (detailObjectImage != null)
            {
                detailObjectImage.rectTransform.localScale = Vector3.one * 0.78f;
                SetImageAlpha(detailObjectImage, 0f);
            }

            if (detailText != null)
            {
                SetTextAlpha(detailText, 0f);
            }

            float duration = Mathf.Max(detailFadeDuration, detailScaleDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float fadeT = Smooth01(Mathf.Clamp01(elapsed / detailFadeDuration));
                float scaleT = Smooth01(Mathf.Clamp01(elapsed / detailScaleDuration));

                SetDetailAlpha(fadeT);
                if (detailObjectImage != null)
                {
                    detailObjectImage.rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.78f, Vector3.one, scaleT);
                }

                yield return null;
            }

            SetDetailAlpha(1f);
            if (detailObjectImage != null)
            {
                detailObjectImage.rectTransform.localScale = Vector3.one;
            }

            detailRoutine = null;
        }

        private IEnumerator HideDetailRoutine()
        {
            if (itemGroup != null && !pageTurned)
            {
                itemGroup.interactable = true;
                itemGroup.blocksRaycasts = true;
            }

            float elapsed = 0f;
            while (elapsed < detailFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Smooth01(Mathf.Clamp01(elapsed / detailFadeDuration));
                SetDetailAlpha(t);

                if (detailObjectImage != null)
                {
                    detailObjectImage.rectTransform.localScale = Vector3.Lerp(Vector3.one * 0.92f, Vector3.one, t);
                }

                yield return null;
            }

            detailOpen = false;
            SetDetailVisible(false, 0f);
            activePageItem = null;
            ApplyDetailContent(null, string.Empty, defaultDetailObjectSize, defaultDetailObjectEulerAngles, false);
            detailRoutine = null;
        }

        private IEnumerator TurnPageRoutine()
        {
            SetPageEdgeButtons(false, false);
            SetWatchSceneInteractable(false);

            if (itemGroup != null)
            {
                itemGroup.interactable = false;
                itemGroup.blocksRaycasts = false;
            }

            if (incomingPageImage != null)
            {
                incomingPageImage.sprite = memoPageOneSprite;
                incomingPageImage.gameObject.SetActive(true);
                incomingPageImage.rectTransform.anchoredPosition = new Vector2(22f, 0f);
                incomingPageImage.rectTransform.localScale = new Vector3(0.985f, 1f, 1f);
                SetImageAlpha(incomingPageImage, 0f);
            }

            if (pageTurnHighlight != null)
            {
                pageTurnHighlight.gameObject.SetActive(true);
                SetImageAlpha(pageTurnHighlight, 0f);
            }

            float elapsed = 0f;
            while (elapsed < pageTurnDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float linear = Mathf.Clamp01(elapsed / pageTurnDuration);
                float t = Smooth01(linear);
                float crest = Mathf.Sin(linear * Mathf.PI);

                if (itemGroup != null)
                {
                    itemGroup.alpha = Mathf.Clamp01(1f - t * 1.18f);
                }

                if (incomingPageImage != null)
                {
                    incomingPageImage.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(22f, 0f, t), 0f);
                    incomingPageImage.rectTransform.localScale = Vector3.Lerp(new Vector3(0.985f, 1f, 1f), Vector3.one, t);
                    SetImageAlpha(incomingPageImage, t);
                }

                if (pageTurnHighlight != null)
                {
                    RectTransform highlightTransform = pageTurnHighlight.rectTransform;
                    highlightTransform.anchoredPosition = new Vector2(Mathf.Lerp(575f, -245f, t), 0f);
                    highlightTransform.sizeDelta = new Vector2(Mathf.Lerp(64f, 230f, crest), 820f);
                    SetImageAlpha(pageTurnHighlight, crest * 0.48f);
                }

                yield return null;
            }

            if (memoImage != null && memoPageOneSprite != null)
            {
                memoImage.sprite = memoPageOneSprite;
            }

            if (incomingPageImage != null)
            {
                SetImageAlpha(incomingPageImage, 0f);
                incomingPageImage.gameObject.SetActive(false);
            }

            if (pageTurnHighlight != null)
            {
                SetImageAlpha(pageTurnHighlight, 0f);
                pageTurnHighlight.gameObject.SetActive(false);
            }

            if (itemGroup != null)
            {
                itemGroup.alpha = 0f;
                itemGroup.gameObject.SetActive(false);
            }

            pageTurned = true;
            PageTurnStateChanged?.Invoke(pageTurned);
            SetPageEdgeButtons(false, true);
            SetWatchSceneInteractable(true);
            pageTurnRoutine = null;
        }

        private IEnumerator TurnBackPageRoutine()
        {
            SetPageEdgeButtons(false, false);
            SetWatchSceneInteractable(false);

            if (itemGroup != null)
            {
                itemGroup.gameObject.SetActive(true);
                itemGroup.alpha = 0f;
                itemGroup.interactable = false;
                itemGroup.blocksRaycasts = false;
            }

            if (incomingPageImage != null)
            {
                incomingPageImage.sprite = memoHomeSprite;
                incomingPageImage.gameObject.SetActive(true);
                incomingPageImage.rectTransform.anchoredPosition = new Vector2(-22f, 0f);
                incomingPageImage.rectTransform.localScale = new Vector3(0.985f, 1f, 1f);
                SetImageAlpha(incomingPageImage, 0f);
            }

            if (pageTurnHighlight != null)
            {
                pageTurnHighlight.gameObject.SetActive(true);
                SetImageAlpha(pageTurnHighlight, 0f);
            }

            float elapsed = 0f;
            while (elapsed < pageTurnDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float linear = Mathf.Clamp01(elapsed / pageTurnDuration);
                float t = Smooth01(linear);
                float crest = Mathf.Sin(linear * Mathf.PI);

                if (incomingPageImage != null)
                {
                    incomingPageImage.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(-22f, 0f, t), 0f);
                    incomingPageImage.rectTransform.localScale = Vector3.Lerp(new Vector3(0.985f, 1f, 1f), Vector3.one, t);
                    SetImageAlpha(incomingPageImage, t);
                }

                if (itemGroup != null)
                {
                    itemGroup.alpha = Mathf.Clamp01((t - 0.32f) / 0.68f);
                }

                if (pageTurnHighlight != null)
                {
                    RectTransform highlightTransform = pageTurnHighlight.rectTransform;
                    highlightTransform.anchoredPosition = new Vector2(Mathf.Lerp(-575f, 245f, t), 0f);
                    highlightTransform.sizeDelta = new Vector2(Mathf.Lerp(64f, 230f, crest), 820f);
                    SetImageAlpha(pageTurnHighlight, crest * 0.48f);
                }

                yield return null;
            }

            if (memoImage != null && memoHomeSprite != null)
            {
                memoImage.sprite = memoHomeSprite;
            }

            if (incomingPageImage != null)
            {
                SetImageAlpha(incomingPageImage, 0f);
                incomingPageImage.gameObject.SetActive(false);
            }

            if (pageTurnHighlight != null)
            {
                SetImageAlpha(pageTurnHighlight, 0f);
                pageTurnHighlight.gameObject.SetActive(false);
            }

            if (itemGroup != null)
            {
                itemGroup.alpha = 1f;
                itemGroup.interactable = true;
                itemGroup.blocksRaycasts = true;
            }

            pageTurned = false;
            PageTurnStateChanged?.Invoke(pageTurned);
            SetPageEdgeButtons(CanTurnForwardPage(), false);
            SetWatchSceneInteractable(true);
            pageTurnRoutine = null;
        }

        private void HandleWatchFirstOpened()
        {
            if (!pageTurned && pageTurnRoutine == null)
            {
                SetPageEdgeButtons(CanTurnForwardPage(), false);
            }
        }

        private bool IsWatchDetailOpen()
        {
            return watchTimeDisplay != null && watchTimeDisplay.IsDetailOpen;
        }

        private bool HasWatchBeenOpened()
        {
            return watchTimeDisplay == null || watchTimeDisplay.HasBeenOpened;
        }

        private bool CanTurnForwardPage()
        {
            return !pageTurned && HasWatchBeenOpened();
        }

        private void SetWatchSceneInteractable(bool interactable)
        {
            if (watchTimeDisplay != null)
            {
                watchTimeDisplay.SetSceneInteractable(interactable);
            }
        }

        private void SetPageEdgeButtons(bool canTurnForward, bool canTurnBackward)
        {
            if (pageEdgeButton != null)
            {
                pageEdgeButton.gameObject.SetActive(canTurnForward);
                pageEdgeButton.interactable = canTurnForward;
            }

            if (previousPageEdgeButton != null)
            {
                previousPageEdgeButton.gameObject.SetActive(canTurnBackward);
                previousPageEdgeButton.interactable = canTurnBackward;
            }
        }

        private void SetDetailVisible(bool visible, float alpha)
        {
            if (detailGroup == null)
            {
                return;
            }

            detailGroup.gameObject.SetActive(visible);
            detailGroup.interactable = visible;
            detailGroup.blocksRaycasts = visible;
            SetDetailAlpha(alpha);
        }

        private void SetDetailAlpha(float alpha)
        {
            if (detailGroup != null)
            {
                detailGroup.alpha = alpha;
            }

            if (detailBackdropImage != null)
            {
                Color color = detailBackdropImage.color;
                color.a = detailBackdropAlpha * alpha;
                detailBackdropImage.color = color;
            }

            SetImageAlpha(detailObjectImage, alpha);

            if (detailText != null)
            {
                SetTextAlpha(detailText, alpha);
            }
        }

        private void CacheDetailObjectDefaults()
        {
            if (detailObjectImage == null)
            {
                return;
            }

            RectTransform rectTransform = detailObjectImage.rectTransform;
            defaultDetailObjectSize = rectTransform.sizeDelta;
            defaultDetailObjectEulerAngles = rectTransform.localEulerAngles;
        }

        private void ConfigureDetailObjectButton()
        {
            if (detailObjectImage == null)
            {
                return;
            }

            if (detailObjectButton == null)
            {
                detailObjectButton = detailObjectImage.GetComponent<Button>();
            }

            if (detailObjectButton == null)
            {
                detailObjectButton = detailObjectImage.gameObject.AddComponent<Button>();
            }

            detailObjectButton.transition = Selectable.Transition.None;
            detailObjectButton.targetGraphic = detailObjectImage;
        }

        private void ToggleActivePageItemSide()
        {
            if (!detailOpen || activePageItem == null || !activePageItem.CanFlipInDetail)
            {
                return;
            }

            activePageItem.ToggleDetailSide();
            ApplyActivePageItemDetail();
        }

        private void ApplyActivePageItemDetail()
        {
            if (activePageItem == null)
            {
                return;
            }

            ApplyDetailContent(
                activePageItem.CurrentDetailSprite,
                activePageItem.CurrentDetailText,
                activePageItem.DetailSize,
                activePageItem.DetailEulerAngles,
                activePageItem.CanFlipInDetail);
        }

        private void ApplyDetailContent(Sprite sprite, string text, Vector2 objectSize, Vector3 objectEulerAngles, bool canFlip)
        {
            if (detailObjectImage != null)
            {
                detailObjectImage.sprite = sprite;
                detailObjectImage.preserveAspect = true;
                detailObjectImage.raycastTarget = canFlip;
                detailObjectImage.rectTransform.sizeDelta = objectSize;
                detailObjectImage.rectTransform.localEulerAngles = objectEulerAngles;
            }

            if (detailObjectButton != null)
            {
                detailObjectButton.interactable = canFlip;
            }

            if (detailText != null)
            {
                detailText.text = text;
            }
        }

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static void SetImageAlpha(Image image, float alpha)
        {
            if (image == null)
            {
                return;
            }

            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        private static void SetTextAlpha(Text text, float alpha)
        {
            if (text == null)
            {
                return;
            }

            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }

    }
}
