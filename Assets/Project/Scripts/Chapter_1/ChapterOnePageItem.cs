using System;
using UnityEngine;
using UnityEngine.UI;

namespace Arendalle
{
    public sealed class ChapterOnePageItem : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private ChapterOneController detailController;
        [SerializeField] private Image sceneImage;
        [SerializeField] private Button sceneButton;
        [SerializeField] private Sprite frontSprite;

        [Header("Detail")]
        [SerializeField] private Sprite backSprite;
        [TextArea]
        [SerializeField] private string frontText = "";
        [TextArea]
        [SerializeField] private string backText = "";
        [SerializeField] private bool canFlipInDetail;
        [SerializeField] private Vector2 detailSize = new Vector2(640f, 520f);
        [SerializeField] private Vector3 detailEulerAngles;

        [Header("Flow")]
        [SerializeField] private string triggerDate = "26.11.22";

        private bool showingBack;

        public Sprite CurrentDetailSprite => showingBack && backSprite != null ? backSprite : frontSprite;
        public string CurrentDetailText => showingBack ? backText : frontText;
        public bool CanFlipInDetail => canFlipInDetail && backSprite != null;
        public Vector2 DetailSize => detailSize;
        public Vector3 DetailEulerAngles => detailEulerAngles;

        private void Awake()
        {
            ResolveReferences();
            RegisterListeners();
            ApplySceneSprite();
        }

        private void OnDestroy()
        {
            if (sceneButton != null)
            {
                sceneButton.onClick.RemoveListener(OpenDetail);
            }
        }

        public void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
            if (visible)
            {
                ResetDetailSide();
                ResolveReferences();
                ApplySceneSprite();
            }
        }

        public bool MatchesTriggerDate(DateTime date)
        {
            return TryParseShortDate(triggerDate, out DateTime targetDate) && date.Date == targetDate.Date;
        }

        public void ResetDetailSide()
        {
            showingBack = false;
        }

        public void ToggleDetailSide()
        {
            if (CanFlipInDetail)
            {
                showingBack = !showingBack;
            }
        }

        private void OpenDetail()
        {
            if (detailController != null)
            {
                detailController.OpenPageItemDetail(this);
            }
        }

        private void ResolveReferences()
        {
            if (sceneImage == null)
            {
                sceneImage = GetComponent<Image>();
            }

            if (sceneButton == null)
            {
                sceneButton = GetComponent<Button>();
            }

            if (detailController == null)
            {
                detailController = FindObjectOfType<ChapterOneController>();
            }
        }

        private void RegisterListeners()
        {
            if (sceneButton == null)
            {
                return;
            }

            sceneButton.onClick.RemoveListener(OpenDetail);
            sceneButton.onClick.AddListener(OpenDetail);
        }

        private void ApplySceneSprite()
        {
            if (sceneImage == null || frontSprite == null)
            {
                return;
            }

            sceneImage.sprite = frontSprite;
            sceneImage.preserveAspect = true;
            sceneImage.raycastTarget = true;
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
