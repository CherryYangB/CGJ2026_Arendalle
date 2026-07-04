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

        [Header("Clickable Items")]
        [SerializeField] private CanvasGroup itemGroup;
        [SerializeField] private Button todoListButton;
        [SerializeField] private RectTransform todoListTransform;
        [SerializeField] private Button blueBarcodeCardButton;
        [SerializeField] private RectTransform blueBarcodeCardTransform;
        [SerializeField] private Button yellowNoteClipButton;
        [SerializeField] private RectTransform yellowNoteClipTransform;

        [Header("Animation")]
        [SerializeField] private float itemShakeDuration = 0.18f;
        [SerializeField] private float itemShakePixels = 8f;
        [SerializeField] private float itemShakeDegrees = 1.8f;
        [SerializeField] private float pageTurnDuration = 0.68f;

        private readonly Dictionary<RectTransform, Coroutine> shakeRoutines = new Dictionary<RectTransform, Coroutine>();
        private Coroutine pageTurnRoutine;
        private bool pageTurned;

        private void Awake()
        {
            if (memoImage != null && memoHomeSprite != null)
            {
                memoImage.sprite = memoHomeSprite;
            }

            SetImageAlpha(incomingPageImage, 0f);
            SetImageAlpha(pageTurnHighlight, 0f);

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
        }

        private void OnDestroy()
        {
            if (todoListButton != null)
            {
                todoListButton.onClick.RemoveListener(ShakeTodoList);
            }

            if (blueBarcodeCardButton != null)
            {
                blueBarcodeCardButton.onClick.RemoveListener(ShakeBlueBarcodeCard);
            }

            if (yellowNoteClipButton != null)
            {
                yellowNoteClipButton.onClick.RemoveListener(ShakeYellowNoteClip);
            }

            if (pageEdgeButton != null)
            {
                pageEdgeButton.onClick.RemoveListener(TurnPage);
            }
        }

        private void RegisterListeners()
        {
            if (todoListButton != null)
            {
                todoListButton.onClick.AddListener(ShakeTodoList);
            }

            if (blueBarcodeCardButton != null)
            {
                blueBarcodeCardButton.onClick.AddListener(ShakeBlueBarcodeCard);
            }

            if (yellowNoteClipButton != null)
            {
                yellowNoteClipButton.onClick.AddListener(ShakeYellowNoteClip);
            }

            if (pageEdgeButton != null)
            {
                pageEdgeButton.onClick.AddListener(TurnPage);
            }
        }

        private void ShakeTodoList()
        {
            ShakeItem(todoListTransform);
        }

        private void ShakeBlueBarcodeCard()
        {
            ShakeItem(blueBarcodeCardTransform);
        }

        private void ShakeYellowNoteClip()
        {
            ShakeItem(yellowNoteClipTransform);
        }

        public void TurnPage()
        {
            if (pageTurned || pageTurnRoutine != null)
            {
                return;
            }

            pageTurnRoutine = StartCoroutine(TurnPageRoutine());
        }

        private void ShakeItem(RectTransform target)
        {
            if (target == null || pageTurned)
            {
                return;
            }

            if (shakeRoutines.TryGetValue(target, out Coroutine runningRoutine) && runningRoutine != null)
            {
                StopCoroutine(runningRoutine);
            }

            shakeRoutines[target] = StartCoroutine(ShakeItemRoutine(target));
        }

        private IEnumerator ShakeItemRoutine(RectTransform target)
        {
            Vector2 basePosition = target.anchoredPosition;
            Quaternion baseRotation = target.localRotation;
            float elapsed = 0f;

            while (elapsed < itemShakeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float normalized = Mathf.Clamp01(elapsed / itemShakeDuration);
                float strength = 1f - Smooth01(normalized);
                float wave = elapsed * 92f;

                target.anchoredPosition = basePosition + new Vector2(
                    Mathf.Sin(wave) * itemShakePixels * strength,
                    Mathf.Cos(wave * 1.2f) * itemShakePixels * 0.45f * strength);
                target.localRotation = baseRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(wave * 0.9f) * itemShakeDegrees * strength);
                yield return null;
            }

            target.anchoredPosition = basePosition;
            target.localRotation = baseRotation;
            shakeRoutines[target] = null;
        }

        private IEnumerator TurnPageRoutine()
        {
            if (pageEdgeButton != null)
            {
                pageEdgeButton.interactable = false;
            }

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
            pageTurnRoutine = null;
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
    }
}
