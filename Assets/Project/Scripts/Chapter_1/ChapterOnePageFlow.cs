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
        [SerializeField] private ChapterOnePageItem weddingInvitation;
        [SerializeField] private ChapterOnePageItem[] movieTickets;
        [SerializeField] private ChapterOnePageItem marriedPhoto;

        [Header("Ending Video")]
        [SerializeField] private VideoClip endingVideoClip;
        [SerializeField] private string nextSceneName = "Chapter_2";
        [SerializeField] private float missingVideoFallbackDelay = 0.25f;

        private bool ticketsUnlocked;
        private bool photoUnlocked;
        private bool endingStarted;

        private void Awake()
        {
            ResolveReferences();
            RegisterListeners();
            ApplyVisibility(false);
        }

        private void OnDestroy()
        {
            if (chapterController != null)
            {
                chapterController.PageTurnStateChanged -= HandlePageTurnStateChanged;
            }

            if (watchTimeDisplay != null)
            {
                watchTimeDisplay.DateChanged -= HandleDateChanged;
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
        }

        private void RegisterListeners()
        {
            if (chapterController != null)
            {
                chapterController.PageTurnStateChanged -= HandlePageTurnStateChanged;
                chapterController.PageTurnStateChanged += HandlePageTurnStateChanged;
            }

            if (watchTimeDisplay != null)
            {
                watchTimeDisplay.DateChanged -= HandleDateChanged;
                watchTimeDisplay.DateChanged += HandleDateChanged;
            }
        }

        private void HandlePageTurnStateChanged(bool isPageTurned)
        {
            ApplyVisibility(isPageTurned);
        }

        private void HandleDateChanged(System.DateTime date)
        {
            if (chapterController == null || !chapterController.IsPageTurned || endingStarted)
            {
                return;
            }

            if (!ticketsUnlocked && weddingInvitation != null && weddingInvitation.MatchesTriggerDate(date))
            {
                ticketsUnlocked = true;
                ApplyVisibility(true);
                return;
            }

            if (!photoUnlocked && ticketsUnlocked && AnyTicketMatches(date))
            {
                photoUnlocked = true;
                ApplyVisibility(true);
                return;
            }

            if (photoUnlocked && marriedPhoto != null && marriedPhoto.MatchesTriggerDate(date))
            {
                StartCoroutine(PlayEndingVideoRoutine());
            }
        }

        private bool AnyTicketMatches(System.DateTime date)
        {
            if (movieTickets == null)
            {
                return false;
            }

            for (int i = 0; i < movieTickets.Length; i++)
            {
                if (movieTickets[i] != null && movieTickets[i].MatchesTriggerDate(date))
                {
                    return true;
                }
            }

            return false;
        }

        private void ApplyVisibility(bool isPageTurned)
        {
            SetItemVisible(weddingInvitation, isPageTurned);

            bool showTickets = isPageTurned && ticketsUnlocked;
            if (movieTickets != null)
            {
                for (int i = 0; i < movieTickets.Length; i++)
                {
                    SetItemVisible(movieTickets[i], showTickets);
                }
            }

            SetItemVisible(marriedPhoto, isPageTurned && photoUnlocked);
        }

        private static void SetItemVisible(ChapterOnePageItem item, bool visible)
        {
            if (item != null)
            {
                item.SetVisible(visible);
            }
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
