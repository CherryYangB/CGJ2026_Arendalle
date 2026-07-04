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

        [Header("Detail Overlay")]
        [SerializeField] private CanvasGroup detailGroup;
        [SerializeField] private Button detailBackdropButton;
        [SerializeField] private Image detailBackdropImage;
        [SerializeField] private Image detailObjectImage;
        [SerializeField] private Text detailText;

        [Header("Animation")]
        [SerializeField] private float detailFadeDuration = 0.24f;
        [SerializeField] private float detailScaleDuration = 0.28f;
        [SerializeField] private float detailBackdropAlpha = 0.68f;
        [SerializeField] private float pageTurnDuration = 0.68f;

        private Coroutine detailRoutine;
        private Coroutine pageTurnRoutine;
        private bool detailOpen;
        private bool pageTurned;

        private void Awake()
        {
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

            SetPageEdgeButtons(true, false);

            RegisterListeners();
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

            if (pageEdgeButton != null)
            {
                pageEdgeButton.onClick.RemoveListener(TurnPage);
            }

            if (previousPageEdgeButton != null)
            {
                previousPageEdgeButton.onClick.RemoveListener(TurnBackPage);
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

            if (pageEdgeButton != null)
            {
                pageEdgeButton.onClick.AddListener(TurnPage);
            }

            if (previousPageEdgeButton != null)
            {
                previousPageEdgeButton.onClick.AddListener(TurnBackPage);
            }
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

            if (detailObjectImage != null)
            {
                detailObjectImage.sprite = sprite;
                detailObjectImage.preserveAspect = true;
            }

            if (detailText != null)
            {
                detailText.text = text;
            }

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
            if (detailOpen || pageTurned || pageTurnRoutine != null)
            {
                return;
            }

            pageTurnRoutine = StartCoroutine(TurnPageRoutine());
        }

        public void TurnBackPage()
        {
            if (detailOpen || !pageTurned || pageTurnRoutine != null)
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
            detailRoutine = null;
        }

        private IEnumerator TurnPageRoutine()
        {
            SetPageEdgeButtons(false, false);

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
            SetPageEdgeButtons(false, true);
            pageTurnRoutine = null;
        }

        private IEnumerator TurnBackPageRoutine()
        {
            SetPageEdgeButtons(false, false);

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
            SetPageEdgeButtons(true, false);
            pageTurnRoutine = null;
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
