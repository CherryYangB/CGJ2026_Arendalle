using System;
using System.Collections;
using System.Collections.Generic;
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
        [SerializeField] private float pageTurnHintBlinkDuration = 1.8f;
        [SerializeField] private float pageTurnHintMaxAlpha = 0.22f;

        [Header("Audio")]
        [SerializeField] private AudioSource[] pauseOnDetailAudioSources;
        [SerializeField] private AudioSource detailAudioSource;
        [SerializeField, Range(0f, 1f)] private float detailAudioVolume = 1f;

        private Coroutine detailRoutine;
        private Coroutine pageTurnRoutine;
        private Coroutine pageTurnHintRoutine;
        private ChapterOnePageItem activePageItem;
        private readonly List<AudioSource> pausedDetailAudioSources = new List<AudioSource>();
        private Vector2 defaultDetailObjectSize;
        private Vector3 defaultDetailObjectEulerAngles;
        private Vector2 defaultPageTurnHighlightPosition;
        private Vector2 defaultPageTurnHighlightSize;
        private bool detailOpen;
        private int currentPageIndex;
        private bool secondPageUnlocked;
        private bool finalPageUnlocked;

        public bool IsPageTurned => currentPageIndex > 0;
        public int CurrentPageIndex => currentPageIndex;
        public event Action<bool> PageTurnStateChanged;
        public event Action<int> PageIndexChanged;
        public event Action FinalPageTurned;

        private void Awake()
        {
            CacheDetailObjectDefaults();
            CachePageTurnHighlightDefaults();
            ConfigureDetailObjectButton();
            ConfigurePageTurnAudioSource();

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

            StopPageItemAudioAndResume();
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
            if (IsPageTurned || pageTurnRoutine != null)
            {
                return;
            }

            activePageItem = null;
            ApplyDetailContent(sprite, text, defaultDetailObjectSize, defaultDetailObjectEulerAngles, false);

            StartDetailRoutine(ShowDetailRoutine());
        }

        public void OpenPageItemDetail(ChapterOnePageItem item)
        {
            if (!IsPageTurned || detailOpen || pageTurnRoutine != null || IsWatchDetailOpen() || item == null)
            {
                return;
            }

            activePageItem = item;
            activePageItem.ResetDetailSide();
            ApplyActivePageItemDetail();
            PlayActivePageItemAudio();
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
            if (detailOpen || pageTurnRoutine != null || IsWatchDetailOpen() || !CanTurnForwardPage())
            {
                return;
            }

            if (currentPageIndex == 2)
            {
                pageTurnRoutine = StartCoroutine(TurnFinalPageRoutine());
                return;
            }

            pageTurnRoutine = StartCoroutine(TurnPageRoutine(currentPageIndex + 1));
        }

        public void TurnBackPage()
        {
            if (detailOpen || !CanTurnBackwardPage() || pageTurnRoutine != null || IsWatchDetailOpen())
            {
                return;
            }

            pageTurnRoutine = StartCoroutine(TurnPageRoutine(currentPageIndex - 1));
        }

        public void SetSecondPageUnlocked(bool unlocked)
        {
            if (secondPageUnlocked == unlocked)
            {
                return;
            }

            secondPageUnlocked = unlocked;

            if (pageTurnRoutine == null && !detailOpen)
            {
                SetPageEdgeButtons(CanTurnForwardPage(), CanTurnBackwardPage());
            }
        }

        public void SetFinalPageUnlocked(bool unlocked)
        {
            if (finalPageUnlocked == unlocked)
            {
                return;
            }

            finalPageUnlocked = unlocked;

            if (pageTurnRoutine == null && !detailOpen)
            {
                SetPageEdgeButtons(CanTurnForwardPage(), CanTurnBackwardPage());
            }
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
            StopPageTurnHint();
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
            StopPageItemAudioAndResume();

            if (itemGroup != null && !IsPageTurned)
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
            SetPageEdgeButtons(CanTurnForwardPage(), CanTurnBackwardPage());
            detailRoutine = null;
        }

        private IEnumerator TurnPageRoutine(int targetPageIndex)
        {
            StopPageTurnHint();
            targetPageIndex = Mathf.Clamp(targetPageIndex, 0, 2);
            int previousPageIndex = currentPageIndex;
            bool movingForward = targetPageIndex > previousPageIndex;
            bool enteringContent = previousPageIndex == 0 && targetPageIndex > 0;
            bool returningHome = targetPageIndex == 0;

            SetPageEdgeButtons(false, false);
            SetWatchSceneInteractable(false);

            if (itemGroup != null)
            {
                if (returningHome)
                {
                    itemGroup.gameObject.SetActive(true);
                    itemGroup.alpha = 0f;
                }

                itemGroup.interactable = false;
                itemGroup.blocksRaycasts = false;
            }

            if (incomingPageImage != null)
            {
                incomingPageImage.sprite = targetPageIndex == 0 ? memoHomeSprite : memoPageOneSprite;
                incomingPageImage.gameObject.SetActive(true);
                incomingPageImage.rectTransform.anchoredPosition = new Vector2(movingForward ? 22f : -22f, 0f);
                incomingPageImage.rectTransform.localScale = new Vector3(0.985f, 1f, 1f);
                SetImageAlpha(incomingPageImage, 0f);
            }

            if (pageTurnHighlight != null)
            {
                pageTurnHighlight.gameObject.SetActive(true);
                SetImageAlpha(pageTurnHighlight, 0f);
            }

            PlayPageTurnAudio();

            float elapsed = 0f;
            while (elapsed < pageTurnDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float linear = Mathf.Clamp01(elapsed / pageTurnDuration);
                float t = Smooth01(linear);
                float crest = Mathf.Sin(linear * Mathf.PI);

                if (itemGroup != null && enteringContent)
                {
                    itemGroup.alpha = Mathf.Clamp01(1f - t * 1.18f);
                }
                else if (itemGroup != null && returningHome)
                {
                    itemGroup.alpha = Mathf.Clamp01((t - 0.32f) / 0.68f);
                }

                if (incomingPageImage != null)
                {
                    float startX = movingForward ? 22f : -22f;
                    incomingPageImage.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(startX, 0f, t), 0f);
                    incomingPageImage.rectTransform.localScale = Vector3.Lerp(new Vector3(0.985f, 1f, 1f), Vector3.one, t);
                    SetImageAlpha(incomingPageImage, t);
                }

                if (pageTurnHighlight != null)
                {
                    RectTransform highlightTransform = pageTurnHighlight.rectTransform;
                    float startX = movingForward ? 575f : -575f;
                    float endX = movingForward ? -245f : 245f;
                    highlightTransform.anchoredPosition = new Vector2(Mathf.Lerp(startX, endX, t), 0f);
                    highlightTransform.sizeDelta = new Vector2(Mathf.Lerp(64f, 230f, crest), 820f);
                    SetImageAlpha(pageTurnHighlight, crest * 0.48f);
                }

                yield return null;
            }

            if (memoImage != null)
            {
                Sprite targetSprite = targetPageIndex == 0 ? memoHomeSprite : memoPageOneSprite;
                if (targetSprite != null)
                {
                    memoImage.sprite = targetSprite;
                }
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
                if (targetPageIndex == 0)
                {
                    itemGroup.gameObject.SetActive(true);
                    itemGroup.alpha = 1f;
                    itemGroup.interactable = true;
                    itemGroup.blocksRaycasts = true;
                }
                else
                {
                    itemGroup.alpha = 0f;
                    itemGroup.gameObject.SetActive(false);
                }
            }

            currentPageIndex = targetPageIndex;
            NotifyPageIndexChanged();
            SetPageEdgeButtons(CanTurnForwardPage(), CanTurnBackwardPage());
            SetWatchSceneInteractable(true);
            pageTurnRoutine = null;
        }

        private IEnumerator TurnFinalPageRoutine()
        {
            StopPageTurnHint();
            SetPageEdgeButtons(false, false);
            SetWatchSceneInteractable(false);

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

            PlayPageTurnAudio();

            float elapsed = 0f;
            while (elapsed < pageTurnDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float linear = Mathf.Clamp01(elapsed / pageTurnDuration);
                float t = Smooth01(linear);
                float crest = Mathf.Sin(linear * Mathf.PI);

                if (incomingPageImage != null)
                {
                    incomingPageImage.rectTransform.anchoredPosition = new Vector2(Mathf.Lerp(22f, 0f, t), 0f);
                    incomingPageImage.rectTransform.localScale = Vector3.Lerp(new Vector3(0.985f, 1f, 1f), Vector3.one, t);
                    SetImageAlpha(incomingPageImage, t * 0.55f);
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

            if (incomingPageImage != null)
            {
                SetImageAlpha(incomingPageImage, 0f);
                incomingPageImage.gameObject.SetActive(false);
            }

            if (pageTurnHighlight != null)
            {
                ResetPageTurnHighlightToDefault();
                SetImageAlpha(pageTurnHighlight, 0f);
                pageTurnHighlight.gameObject.SetActive(false);
            }

            pageTurnRoutine = null;
            FinalPageTurned?.Invoke();
        }

        private void HandleWatchFirstOpened()
        {
            if (currentPageIndex == 0 && pageTurnRoutine == null)
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
            if (currentPageIndex == 0)
            {
                return HasWatchBeenOpened();
            }

            if (currentPageIndex == 1)
            {
                return secondPageUnlocked;
            }

            if (currentPageIndex == 2)
            {
                return finalPageUnlocked;
            }

            return false;
        }

        private bool CanTurnBackwardPage()
        {
            return currentPageIndex > 0;
        }

        private void NotifyPageIndexChanged()
        {
            PageTurnStateChanged?.Invoke(IsPageTurned);
            PageIndexChanged?.Invoke(currentPageIndex);
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

            if (canTurnForward && pageTurnRoutine == null && !detailOpen)
            {
                StartPageTurnHint();
            }
            else
            {
                StopPageTurnHint();
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

        private void CachePageTurnHighlightDefaults()
        {
            if (pageTurnHighlight == null)
            {
                return;
            }

            RectTransform rectTransform = pageTurnHighlight.rectTransform;
            defaultPageTurnHighlightPosition = rectTransform.anchoredPosition;
            defaultPageTurnHighlightSize = rectTransform.sizeDelta;
        }

        private void StartPageTurnHint()
        {
            if (pageTurnHighlight == null || pageTurnHintRoutine != null)
            {
                return;
            }

            ResetPageTurnHighlightToDefault();
            pageTurnHighlight.gameObject.SetActive(true);
            pageTurnHintRoutine = StartCoroutine(PageTurnHintRoutine());
        }

        private void StopPageTurnHint()
        {
            if (pageTurnHintRoutine != null)
            {
                StopCoroutine(pageTurnHintRoutine);
                pageTurnHintRoutine = null;
            }

            if (pageTurnHighlight != null)
            {
                SetImageAlpha(pageTurnHighlight, 0f);
                pageTurnHighlight.gameObject.SetActive(false);
            }
        }

        private IEnumerator PageTurnHintRoutine()
        {
            float blinkDuration = Mathf.Max(0.1f, pageTurnHintBlinkDuration);
            while (true)
            {
                float phase = Mathf.PingPong(Time.unscaledTime / blinkDuration, 1f);
                float alpha = Smooth01(phase) * pageTurnHintMaxAlpha;
                SetImageAlpha(pageTurnHighlight, alpha);
                yield return null;
            }
        }

        private void ResetPageTurnHighlightToDefault()
        {
            if (pageTurnHighlight == null)
            {
                return;
            }

            RectTransform rectTransform = pageTurnHighlight.rectTransform;
            rectTransform.anchoredPosition = defaultPageTurnHighlightPosition;
            rectTransform.sizeDelta = defaultPageTurnHighlightSize;
        }

        private void ConfigurePageTurnAudioSource()
        {
            AudioSource pageTurnAudioSource = GetPageTurnAudioSource();
            if (pageTurnAudioSource == null)
            {
                return;
            }

            pageTurnAudioSource.playOnAwake = false;
            pageTurnAudioSource.loop = false;
            pageTurnAudioSource.spatialBlend = 0f;
        }

        private void PlayPageTurnAudio()
        {
            AudioSource pageTurnAudioSource = GetPageTurnAudioSource();
            if (pageTurnAudioSource == null || pageTurnAudioSource.clip == null)
            {
                return;
            }

            pageTurnAudioSource.Stop();
            pageTurnAudioSource.Play();
        }

        private AudioSource GetPageTurnAudioSource()
        {
            return pageTurnHighlight != null ? pageTurnHighlight.GetComponent<AudioSource>() : null;
        }

        private void PlayActivePageItemAudio()
        {
            PauseDetailAudioSources();

            AudioClip clip = activePageItem != null ? activePageItem.DetailAudioClip : null;
            if (clip == null)
            {
                return;
            }

            AudioSource source = EnsureDetailAudioSource();
            if (source == null)
            {
                return;
            }

            source.Stop();
            source.clip = clip;
            source.volume = detailAudioVolume;
            source.loop = false;
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.Play();
        }

        private void PauseDetailAudioSources()
        {
            pausedDetailAudioSources.Clear();
            if (pauseOnDetailAudioSources == null)
            {
                return;
            }

            foreach (AudioSource source in pauseOnDetailAudioSources)
            {
                if (source == null || source == detailAudioSource || !source.isPlaying)
                {
                    continue;
                }

                source.Pause();
                pausedDetailAudioSources.Add(source);
            }
        }

        private void StopPageItemAudioAndResume()
        {
            if (detailAudioSource != null)
            {
                detailAudioSource.Stop();
            }

            ResumePausedDetailAudioSources();
        }

        private void ResumePausedDetailAudioSources()
        {
            foreach (AudioSource source in pausedDetailAudioSources)
            {
                if (source != null)
                {
                    source.UnPause();
                }
            }

            pausedDetailAudioSources.Clear();
        }

        private AudioSource EnsureDetailAudioSource()
        {
            if (detailAudioSource == null)
            {
                detailAudioSource = GetComponent<AudioSource>();
            }

            if (detailAudioSource == null)
            {
                detailAudioSource = gameObject.AddComponent<AudioSource>();
            }

            return detailAudioSource;
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
