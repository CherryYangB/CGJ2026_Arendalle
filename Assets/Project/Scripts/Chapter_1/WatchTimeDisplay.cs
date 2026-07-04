using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Arendalle
{
    public sealed class WatchTimeDisplay : MonoBehaviour
    {
        [Header("Scene Watch")]
        [SerializeField] private RectTransform sceneWatchTransform;
        [SerializeField] private Button sceneWatchButton;
        [SerializeField] private Text sceneDateText;

        [Header("Detail Watch")]
        [SerializeField] private CanvasGroup detailGroup;
        [SerializeField] private Image detailBackdropImage;
        [SerializeField] private Button detailBackdropButton;
        [SerializeField] private RectTransform detailWatchTransform;
        [SerializeField] private Text detailDateText;
        [SerializeField] private InputField detailDateInputField;
        [SerializeField] private bool syncDetailDateLayoutFromScene = true;

        [Header("Position")]
        [SerializeField] private Vector2 dockedPosition = new Vector2(-835f, -365f);
        [SerializeField] private Vector2 dockedSize = new Vector2(204f, 1122f);
        [SerializeField] private float dockedRotation = -28f;

        [Header("Animation")]
        [SerializeField] private float fadeDuration = 0.24f;
        [SerializeField] private float detailScaleDuration = 0.28f;
        [SerializeField] private float detailBackdropAlpha = 0.68f;

        private DateTime displayedDate;
        private Coroutine detailRoutine;
        private bool detailOpen;
        private bool docked;
        private bool requestedSceneInteractable = true;
        private bool suppressDateInputCommit;

        public bool IsDetailOpen => detailOpen;
        public bool HasBeenOpened { get; private set; }
        public event Action FirstOpened;

        private void Awake()
        {
            displayedDate = DateTime.Now.Date;
            SetDetailVisible(false, 0f);
            ConfigureDateInputField();
            RegisterListeners();
            ApplySceneInteractable();
            UpdateDateTexts();
            SyncDetailDateLayoutFromScene();
        }

        private void OnDestroy()
        {
            if (sceneWatchButton != null)
            {
                sceneWatchButton.onClick.RemoveListener(OpenDetail);
            }

            if (detailBackdropButton != null)
            {
                detailBackdropButton.onClick.RemoveListener(CloseDetail);
            }

            if (detailDateInputField != null)
            {
                detailDateInputField.onEndEdit.RemoveListener(CommitDateInput);
            }
        }

        public void SetSceneInteractable(bool interactable)
        {
            requestedSceneInteractable = interactable;
            ApplySceneInteractable();
        }

        public void OpenDetail()
        {
            if (detailOpen || detailRoutine != null)
            {
                return;
            }

            detailOpen = true;
            if (!HasBeenOpened)
            {
                HasBeenOpened = true;
                FirstOpened?.Invoke();
            }

            ApplySceneInteractable();
            SyncDetailDateLayoutFromScene();
            SetSceneWatchVisible(false);
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

        private void RegisterListeners()
        {
            if (sceneWatchButton != null)
            {
                sceneWatchButton.onClick.RemoveListener(OpenDetail);
                sceneWatchButton.onClick.AddListener(OpenDetail);
            }

            if (detailBackdropButton != null)
            {
                detailBackdropButton.onClick.RemoveListener(CloseDetail);
                detailBackdropButton.onClick.AddListener(CloseDetail);
            }

            if (detailDateInputField != null)
            {
                detailDateInputField.onEndEdit.RemoveListener(CommitDateInput);
                detailDateInputField.onEndEdit.AddListener(CommitDateInput);
            }
        }

        private IEnumerator ShowDetailRoutine()
        {
            SetDetailVisible(true, 0f);

            if (detailWatchTransform != null)
            {
                detailWatchTransform.localScale = Vector3.one * 0.82f;
            }

            float duration = Mathf.Max(fadeDuration, detailScaleDuration);
            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float fadeT = Smooth01(Mathf.Clamp01(elapsed / fadeDuration));
                float scaleT = Smooth01(Mathf.Clamp01(elapsed / detailScaleDuration));
                SetDetailAlpha(fadeT);

                if (detailWatchTransform != null)
                {
                    detailWatchTransform.localScale = Vector3.Lerp(Vector3.one * 0.82f, Vector3.one, scaleT);
                }

                yield return null;
            }

            SetDetailAlpha(1f);
            if (detailWatchTransform != null)
            {
                detailWatchTransform.localScale = Vector3.one;
            }

            detailRoutine = null;
        }

        private IEnumerator HideDetailRoutine()
        {
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = 1f - Smooth01(Mathf.Clamp01(elapsed / fadeDuration));
                SetDetailAlpha(t);

                if (detailWatchTransform != null)
                {
                    detailWatchTransform.localScale = Vector3.Lerp(Vector3.one * 0.92f, Vector3.one, t);
                }

                yield return null;
            }

            detailOpen = false;
            SetDetailVisible(false, 0f);

            if (!docked)
            {
                DockSceneWatch();
            }

            SetSceneWatchVisible(true);
            ApplySceneInteractable();
            detailRoutine = null;
        }

        private void StartDetailRoutine(IEnumerator routine)
        {
            if (detailRoutine != null)
            {
                StopCoroutine(detailRoutine);
            }

            detailRoutine = StartCoroutine(routine);
        }

        private void DockSceneWatch()
        {
            if (sceneWatchTransform == null)
            {
                return;
            }

            docked = true;
            SetRect(sceneWatchTransform, new Vector2(0.5f, 0.5f), dockedSize, dockedPosition);
            sceneWatchTransform.localEulerAngles = new Vector3(0f, 0f, dockedRotation);
        }

        private void ConfigureDateInputField()
        {
            if (detailDateText == null)
            {
                return;
            }

            if (detailDateInputField == null)
            {
                detailDateInputField = detailDateText.GetComponent<InputField>();
            }

            if (detailDateInputField == null)
            {
                detailDateInputField = detailDateText.gameObject.AddComponent<InputField>();
            }

            detailDateText.raycastTarget = true;
            detailDateInputField.textComponent = detailDateText;
            detailDateInputField.targetGraphic = detailDateText;
            detailDateInputField.transition = Selectable.Transition.None;
            detailDateInputField.contentType = InputField.ContentType.Standard;
            detailDateInputField.lineType = InputField.LineType.SingleLine;
            detailDateInputField.characterLimit = 8;
        }

        private void SyncDetailDateLayoutFromScene()
        {
            if (!syncDetailDateLayoutFromScene
                || sceneWatchTransform == null
                || sceneDateText == null
                || detailWatchTransform == null
                || detailDateText == null)
            {
                return;
            }

            RectTransform source = sceneDateText.rectTransform;
            RectTransform target = detailDateText.rectTransform;
            Vector2 sourceParentSize = sceneWatchTransform.rect.size;
            Vector2 targetParentSize = detailWatchTransform.rect.size;

            if (Mathf.Approximately(sourceParentSize.x, 0f) || Mathf.Approximately(sourceParentSize.y, 0f))
            {
                return;
            }

            Vector2 scale = new Vector2(targetParentSize.x / sourceParentSize.x, targetParentSize.y / sourceParentSize.y);
            target.anchorMin = source.anchorMin;
            target.anchorMax = source.anchorMax;
            target.pivot = source.pivot;
            target.anchoredPosition = new Vector2(source.anchoredPosition.x * scale.x, source.anchoredPosition.y * scale.y);
            target.sizeDelta = source.sizeDelta;
            target.localRotation = source.localRotation;
            target.localScale = source.localScale;
            detailDateText.fontSize = sceneDateText.fontSize;
            detailDateText.resizeTextMinSize = sceneDateText.resizeTextMinSize;
            detailDateText.resizeTextMaxSize = sceneDateText.resizeTextMaxSize;
        }

        private void UpdateDateTexts()
        {
            string value = displayedDate.ToString("yy.MM.dd");

            if (sceneDateText != null)
            {
                sceneDateText.text = value;
            }

            if (detailDateText != null)
            {
                detailDateText.text = value;
            }

            if (detailDateInputField != null)
            {
                suppressDateInputCommit = true;
                detailDateInputField.text = value;
                suppressDateInputCommit = false;
            }
        }

        private void CommitDateInput(string input)
        {
            if (suppressDateInputCommit)
            {
                return;
            }

            if (TryParseShortDate(input, out DateTime parsedDate))
            {
                displayedDate = parsedDate;
            }

            UpdateDateTexts();
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

        private void ApplySceneInteractable()
        {
            if (sceneWatchButton != null)
            {
                sceneWatchButton.interactable = requestedSceneInteractable && !detailOpen;
            }
        }

        private void SetSceneWatchVisible(bool visible)
        {
            if (sceneWatchTransform != null)
            {
                sceneWatchTransform.gameObject.SetActive(visible);
            }
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

        private static float Smooth01(float value)
        {
            value = Mathf.Clamp01(value);
            return value * value * (3f - 2f * value);
        }

        private static bool TryParseShortDate(string value, out DateTime parsedDate)
        {
            parsedDate = default;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string[] parts = value.Trim().Split('.');
            if (parts.Length != 3
                || !int.TryParse(parts[0], out int year)
                || !int.TryParse(parts[1], out int month)
                || !int.TryParse(parts[2], out int day))
            {
                return false;
            }

            if (parts[0].Length == 2)
            {
                year += 2000;
            }

            try
            {
                parsedDate = new DateTime(year, month, day);
                return true;
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }
        }
    }
}
