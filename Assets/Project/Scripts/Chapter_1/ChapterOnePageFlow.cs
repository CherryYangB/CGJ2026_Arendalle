using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Video;

namespace Arendalle
{
    public sealed class ChapterOnePageFlow : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private ChapterOneController chapterController;
        [SerializeField] private WatchTimeDisplay watchTimeDisplay;

        [Header("Page 1")]
        [SerializeField] private ChapterOnePageItem memoDay;
        [SerializeField] private ChapterOnePageItem[] movieTickets;
        [SerializeField] private Text firstPageText;
        [SerializeField] private Text ticketCaptionText;
        [SerializeField] private ChapterOnePageItem memoDayDateGateItem;
        [SerializeField] private string memoDayUnlockDate = "20:07";
        [SerializeField] private string movieTicketUnlockDate = "03:05";

        [Header("Page 2")]
        [SerializeField] private ChapterOnePageItem weddingInvitation;
        [SerializeField] private ChapterOnePageItem marriedPhoto;
        [SerializeField] private Text secondPageText;
        [SerializeField] private ChapterOnePageItem weddingInvitationDateGateItem;
        [SerializeField] private string weddingInvitationUnlockDate = "11:22";
        [SerializeField] private ChapterOnePageItem marriedPhotoDateGateItem;
        [SerializeField] private string marriedPhotoEndingDate = "05:20";

        [Header("Ending Video")]
        [SerializeField] private VideoClip endingVideoClip;
        [SerializeField] private string nextSceneName = "Chapter_2";
        [SerializeField] private float missingVideoFallbackDelay = 0.25f;

        private bool memoDayDateCompleted;
        private bool movieTicketDateCompleted;
        private bool marriedPhotoDateCompleted;
        private bool weddingInvitationDateCompleted;
        private bool endingStarted;

        private void Awake()
        {
            ResolveReferences();
            RegisterListeners();
            ApplyVisibility();
            UpdateControllerUnlock();
        }

        private void OnDestroy()
        {
            if (chapterController != null)
            {
                chapterController.PageIndexChanged -= HandlePageIndexChanged;
                chapterController.FinalPageTurned -= HandleFinalPageTurned;
            }

            if (watchTimeDisplay != null)
            {
                watchTimeDisplay.TimeChanged -= HandleTimeChanged;
            }
        }

        private void ResolveReferences()
        {
            if (chapterController == null)
            {
                chapterController = FindObjectOfType<ChapterOneController>();
            }

            if (watchTimeDisplay == null)
            {
                watchTimeDisplay = FindObjectOfType<WatchTimeDisplay>();
            }

            if (memoDay == null)
            {
                memoDay = FindPageItem("MemoDay");
            }

            if (movieTickets == null || movieTickets.Length == 0)
            {
                movieTickets = new[]
                {
                    FindPageItem("MovieTicket1"),
                    FindPageItem("MovieTicket2")
                };
            }

            if (weddingInvitation == null)
            {
                weddingInvitation = FindPageItem("WeddingInvitationCard");
            }

            if (marriedPhoto == null)
            {
                marriedPhoto = FindPageItem("MarriedPhoto");
            }

            if (firstPageText == null)
            {
                firstPageText = FindText("FirstPageText");
            }

            if (ticketCaptionText == null)
            {
                ticketCaptionText = FindText("TicketCaptionText");
            }

            if (secondPageText == null)
            {
                secondPageText = FindText("SecondPageText");
            }

            if (memoDayDateGateItem == null)
            {
                memoDayDateGateItem = memoDay;
            }

            if (weddingInvitationDateGateItem == null)
            {
                weddingInvitationDateGateItem = weddingInvitation;
            }

            if (marriedPhotoDateGateItem == null)
            {
                marriedPhotoDateGateItem = marriedPhoto;
            }
        }

        private void RegisterListeners()
        {
            if (chapterController != null)
            {
                chapterController.PageIndexChanged -= HandlePageIndexChanged;
                chapterController.PageIndexChanged += HandlePageIndexChanged;
                chapterController.FinalPageTurned -= HandleFinalPageTurned;
                chapterController.FinalPageTurned += HandleFinalPageTurned;
            }

            if (watchTimeDisplay != null)
            {
                watchTimeDisplay.TimeChanged -= HandleTimeChanged;
                watchTimeDisplay.TimeChanged += HandleTimeChanged;
            }
        }

        private void HandlePageIndexChanged(int pageIndex)
        {
            ApplyVisibility();
            UpdateControllerUnlock();
        }

        private void HandleTimeChanged(TimeSpan time)
        {
            if (chapterController == null || endingStarted)
            {
                return;
            }

            if (chapterController.CurrentPageIndex == 1)
            {
                bool changed = false;
                if (!memoDayDateCompleted && MatchesMemoDayTime(time))
                {
                    memoDayDateCompleted = true;
                    changed = true;
                }

                if (memoDayDateCompleted && !movieTicketDateCompleted && MatchesMovieTicketTime(time))
                {
                    movieTicketDateCompleted = true;
                    changed = true;
                }

                if (changed)
                {
                    ApplyVisibility();
                    UpdateControllerUnlock();
                }

                return;
            }

            if (chapterController.CurrentPageIndex == 2)
            {
                bool changed = false;
                if (!marriedPhotoDateCompleted && MatchesMarriedPhotoTime(time))
                {
                    marriedPhotoDateCompleted = true;
                    changed = true;
                }

                if (marriedPhotoDateCompleted && !weddingInvitationDateCompleted && MatchesWeddingInvitationTime(time))
                {
                    weddingInvitationDateCompleted = true;
                    changed = true;
                }

                if (changed)
                {
                    ApplyVisibility();
                    UpdateControllerUnlock();
                }
            }
        }

        private void HandleFinalPageTurned()
        {
            StartCoroutine(PlayEndingVideoRoutine());
        }

        private bool MatchesMemoDayTime(TimeSpan time)
        {
            return MatchesGateItem(memoDayDateGateItem, time) || MatchesClockTime(memoDayUnlockDate, time);
        }

        private bool MatchesMovieTicketTime(TimeSpan time)
        {
            return MatchesAnyGateItem(movieTickets, time) || MatchesClockTime(movieTicketUnlockDate, time);
        }

        private bool MatchesWeddingInvitationTime(TimeSpan time)
        {
            return MatchesGateItem(weddingInvitationDateGateItem, time) || MatchesClockTime(weddingInvitationUnlockDate, time);
        }

        private bool MatchesMarriedPhotoTime(TimeSpan time)
        {
            return MatchesGateItem(marriedPhotoDateGateItem, time) || MatchesClockTime(marriedPhotoEndingDate, time);
        }

        private static bool MatchesGateItem(ChapterOnePageItem item, TimeSpan time)
        {
            return item != null && item.MatchesTriggerTime(time);
        }

        private static bool MatchesAnyGateItem(ChapterOnePageItem[] items, TimeSpan time)
        {
            if (items == null)
            {
                return false;
            }

            for (int i = 0; i < items.Length; i++)
            {
                if (MatchesGateItem(items[i], time))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool MatchesClockTime(string value, TimeSpan time)
        {
            return TryParseClockTime(value, out TimeSpan targetTime)
                && targetTime.Hours == time.Hours
                && targetTime.Minutes == time.Minutes;
        }

        private void ApplyVisibility()
        {
            int pageIndex = chapterController != null ? chapterController.CurrentPageIndex : 0;
            bool showFirstPage = pageIndex == 1;
            bool showSecondPage = pageIndex == 2;
            bool showMovieTickets = showFirstPage && memoDayDateCompleted;
            bool showWeddingInvitation = showSecondPage && marriedPhotoDateCompleted;

            SetItemVisible(memoDay, showFirstPage);
            SetItemsVisible(movieTickets, showMovieTickets);
            SetTextVisible(firstPageText, showFirstPage);
            SetTextVisible(ticketCaptionText, showFirstPage);

            SetItemVisible(marriedPhoto, showSecondPage);
            SetItemVisible(weddingInvitation, showWeddingInvitation);
            SetTextVisible(secondPageText, showSecondPage);
        }

        private void UpdateControllerUnlock()
        {
            if (chapterController != null)
            {
                chapterController.SetSecondPageUnlocked(memoDayDateCompleted && movieTicketDateCompleted);
                chapterController.SetFinalPageUnlocked(marriedPhotoDateCompleted && weddingInvitationDateCompleted);
            }
        }

        private static void SetItemsVisible(ChapterOnePageItem[] items, bool visible)
        {
            if (items == null)
            {
                return;
            }

            for (int i = 0; i < items.Length; i++)
            {
                SetItemVisible(items[i], visible);
            }
        }

        private static void SetItemVisible(ChapterOnePageItem item, bool visible)
        {
            if (item != null)
            {
                item.SetVisible(visible);
            }
        }

        private static void SetTextVisible(Text text, bool visible)
        {
            if (text != null)
            {
                text.gameObject.SetActive(visible);
            }
        }

        private static ChapterOnePageItem FindPageItem(string objectName)
        {
            ChapterOnePageItem[] items = Resources.FindObjectsOfTypeAll<ChapterOnePageItem>();
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i] != null && items[i].name == objectName)
                {
                    return items[i];
                }
            }

            return null;
        }

        private static Text FindText(string objectName)
        {
            Text[] texts = Resources.FindObjectsOfTypeAll<Text>();
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null && texts[i].name == objectName)
                {
                    return texts[i];
                }
            }

            return null;
        }

        private static bool TryParseClockTime(string value, out TimeSpan parsedTime)
        {
            parsedTime = default;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            string[] parts = value.Trim().Split(':', '.');
            if (parts.Length != 2
                || parts[0].Length != 2
                || parts[1].Length != 2
                || !int.TryParse(parts[0], out int hour)
                || !int.TryParse(parts[1], out int minute)
                || hour < 0
                || hour > 23
                || minute < 0
                || minute > 59)
            {
                return false;
            }

            parsedTime = new TimeSpan(hour, minute, 0);
            return true;
        }

        private IEnumerator PlayEndingVideoRoutine()
        {
            if (endingStarted)
            {
                yield break;
            }

            endingStarted = true;

            if (endingVideoClip == null)
            {
                yield return new WaitForSecondsRealtime(missingVideoFallbackDelay);
                LoadNextScene();
                yield break;
            }

            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                LoadNextScene();
                yield break;
            }

            GameObject overlay = new GameObject("EndingVideoOverlay", typeof(RectTransform), typeof(RawImage), typeof(VideoPlayer));
            overlay.transform.SetParent(canvas.transform, false);
            overlay.transform.SetAsLastSibling();

            RectTransform rectTransform = overlay.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            RawImage rawImage = overlay.GetComponent<RawImage>();
            rawImage.color = Color.black;
            rawImage.raycastTarget = true;

            RenderTexture renderTexture = new RenderTexture(Screen.width, Screen.height, 0);
            rawImage.texture = renderTexture;

            VideoPlayer videoPlayer = overlay.GetComponent<VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = false;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
            videoPlayer.clip = endingVideoClip;
            videoPlayer.Prepare();

            while (!videoPlayer.isPrepared)
            {
                yield return null;
            }

            rawImage.color = Color.white;
            videoPlayer.Play();

            while (videoPlayer.isPlaying)
            {
                yield return null;
            }

            renderTexture.Release();
            LoadNextScene();
        }

        private void LoadNextScene()
        {
            if (!string.IsNullOrWhiteSpace(nextSceneName) && Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                SceneManager.LoadScene(nextSceneName);
                return;
            }

            Debug.LogWarning($"Cannot load scene '{nextSceneName}'. Add it to Build Settings when Chapter_2 is ready.");
        }
    }
}
