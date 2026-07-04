using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Arendalle
{
    public sealed class MainMenuController : MonoBehaviour
    {
        [Header("Scene")]
        [SerializeField] private string chapterSceneName = "Chapter_1";

        [Header("Buttons")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button aboutButton;

        [Header("Groups")]
        [SerializeField] private CanvasGroup homeGroup;
        [SerializeField] private CanvasGroup aboutGroup;

        [Header("Transition")]
        [SerializeField] private Image whiteFade;
        [SerializeField] private CanvasGroup transitionTextGroup;
        [SerializeField] private Text transitionText;
        [TextArea]
        [SerializeField] private string transitionMessage = "";
        [TextArea]
        [SerializeField] private string transitionMessage2 = "";
        [SerializeField] private float sceneFadeDuration = 1.15f;
        [SerializeField] private float transitionTextHoldDuration = 0.8f;
        [SerializeField] private float transitionTextFadeOutDuration = 0.75f;
        [SerializeField] private float aboutFadeDuration = 0.9f;

        [Header("Music Placeholder")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioClip musicClip;

        [Header("Text")]
        [SerializeField] private string[] preferredFontNames =
        {
            "PingFang SC",
            "Hiragino Sans GB",
            "Microsoft YaHei",
            "Noto Sans CJK SC",
            "Arial Unicode MS",
            "Arial"
        };

        private Coroutine runningRoutine;
        private bool isAboutVisible;

        private void Awake()
        {
            ResolveTransitionTextReferences();
            ApplyRuntimeFont();

            if (startButton != null)
            {
                startButton.onClick.AddListener(StartGame);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(QuitGame);
            }

            if (aboutButton != null)
            {
                aboutButton.onClick.AddListener(ShowAbout);
            }

            if (homeGroup != null)
            {
                SetGroup(homeGroup, 1f, true);
            }

            if (aboutGroup != null)
            {
                SetGroup(aboutGroup, 0f, false);
            }

            SetFadeAlpha(0f);
            SetTransitionTextAlpha(0f);
            SetTransitionMessage(string.Empty);
            PrepareMusic();
        }

        private void OnDestroy()
        {
            if (startButton != null)
            {
                startButton.onClick.RemoveListener(StartGame);
            }

            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(QuitGame);
            }

            if (aboutButton != null)
            {
                aboutButton.onClick.RemoveListener(ShowAbout);
            }
        }

        private void Update()
        {
            if (isAboutVisible && runningRoutine == null && Input.GetKeyDown(KeyCode.Escape))
            {
                HideAbout();
            }
        }

        private void ApplyRuntimeFont()
        {
            if (preferredFontNames == null || preferredFontNames.Length == 0)
            {
                return;
            }

            Font font = Font.CreateDynamicFontFromOSFont(preferredFontNames, 32);
            if (font == null)
            {
                return;
            }

            Text[] texts = FindObjectsOfType<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                texts[i].font = font;
            }
        }

        public void StartGame()
        {
            StartExclusiveRoutine(FadeToWhiteAndLoad());
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        public void ShowAbout()
        {
            StartExclusiveRoutine(ShowAboutRoutine());
        }

        public void HideAbout()
        {
            if (!isAboutVisible)
            {
                return;
            }

            StartExclusiveRoutine(HideAboutRoutine());
        }

        private void PrepareMusic()
        {
            if (musicSource == null)
            {
                return;
            }

            musicSource.loop = true;
            musicSource.playOnAwake = false;

            if (musicClip != null)
            {
                musicSource.clip = musicClip;
            }

            if (musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }

        private void StartExclusiveRoutine(IEnumerator routine)
        {
            if (runningRoutine != null)
            {
                StopCoroutine(runningRoutine);
            }

            runningRoutine = StartCoroutine(routine);
        }

        private IEnumerator FadeToWhiteAndLoad()
        {
            SetMenuInteractable(false);
            StopTransitionMusic();
            BringTransitionTextToFront();

            float elapsed = 0f;
            while (elapsed < sceneFadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float alpha = Mathf.Clamp01(elapsed / sceneFadeDuration);
                SetFadeAlpha(alpha);
                SetTransitionTextAlpha(alpha);
                yield return null;
            }

            SetFadeAlpha(1f);
            yield return PlayTransitionStage(transitionMessage, true);
            yield return PlayTransitionStage(transitionMessage2, false);

            SceneManager.LoadScene(chapterSceneName);
        }

        private IEnumerator ShowAboutRoutine()
        {
            isAboutVisible = false;
            SetMenuInteractable(false);

            yield return FadeGroup(homeGroup, 1f, 0f, aboutFadeDuration);
            SetGroup(homeGroup, 0f, false);

            SetGroup(aboutGroup, 0f, false);
            yield return FadeGroup(aboutGroup, 0f, 1f, aboutFadeDuration);
            SetGroup(aboutGroup, 1f, true);

            isAboutVisible = true;
            runningRoutine = null;
        }

        private IEnumerator HideAboutRoutine()
        {
            isAboutVisible = false;
            yield return FadeGroup(aboutGroup, 1f, 0f, aboutFadeDuration);
            SetGroup(aboutGroup, 0f, false);

            yield return FadeGroup(homeGroup, 0f, 1f, aboutFadeDuration);
            SetGroup(homeGroup, 1f, true);
            SetMenuInteractable(true);

            runningRoutine = null;
        }

        private IEnumerator FadeGroup(CanvasGroup group, float from, float to, float duration)
        {
            if (group == null)
            {
                yield break;
            }

            float elapsed = 0f;
            group.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            group.alpha = to;
        }

        private void SetMenuInteractable(bool isInteractable)
        {
            if (startButton != null)
            {
                startButton.interactable = isInteractable;
            }

            if (quitButton != null)
            {
                quitButton.interactable = isInteractable;
            }

            if (aboutButton != null)
            {
                aboutButton.interactable = isInteractable;
            }
        }

        private void SetGroup(CanvasGroup group, float alpha, bool interactable)
        {
            if (group == null)
            {
                return;
            }

            group.alpha = alpha;
            group.interactable = interactable;
            group.blocksRaycasts = interactable;
        }

        private void SetFadeAlpha(float alpha)
        {
            if (whiteFade == null)
            {
                return;
            }

            Color color = whiteFade.color;
            color.a = alpha;
            whiteFade.color = color;
            whiteFade.raycastTarget = alpha > 0.01f;
        }

        private IEnumerator HoldTransitionText()
        {
            if (transitionTextHoldDuration > 0f)
            {
                yield return new WaitForSecondsRealtime(transitionTextHoldDuration);
            }
        }

        private IEnumerator PlayTransitionStage(string message, bool includeFadeIn)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                yield break;
            }

            SetTransitionMessage(message);

            if (includeFadeIn)
            {
                SetTransitionTextAlpha(1f);
            }
            else
            {
                yield return FadeTransitionText(0f, 1f, sceneFadeDuration);
            }

            PlayTransitionMusic();
            yield return HoldTransitionText();
            StopTransitionMusic();
            yield return FadeTransitionText(1f, 0f, transitionTextFadeOutDuration);
        }

        private IEnumerator FadeTransitionText(float from, float to, float duration)
        {
            if (transitionTextGroup == null && transitionText == null)
            {
                yield break;
            }

            float elapsed = 0f;
            SetTransitionTextAlpha(from);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                SetTransitionTextAlpha(Mathf.Lerp(from, to, Mathf.Clamp01(elapsed / duration)));
                yield return null;
            }

            SetTransitionTextAlpha(to);
        }

        private void SetTransitionTextAlpha(float alpha)
        {
            if (transitionTextGroup != null)
            {
                transitionTextGroup.alpha = alpha;
                transitionTextGroup.interactable = false;
                transitionTextGroup.blocksRaycasts = false;
            }

            if (transitionText != null)
            {
                Color color = transitionText.color;
                color.a = alpha;
                transitionText.color = color;
            }
        }

        private void SetTransitionMessage(string message)
        {
            if (transitionText != null)
            {
                transitionText.text = message;
            }
        }

        private void ResolveTransitionTextReferences()
        {
            if (transitionText == null)
            {
                GameObject transitionObject = GameObject.Find("TransitionText");
                if (transitionObject != null)
                {
                    transitionText = transitionObject.GetComponent<Text>();
                    transitionTextGroup = transitionObject.GetComponent<CanvasGroup>();
                }
            }

            if (transitionText != null && transitionTextGroup == null)
            {
                transitionTextGroup = transitionText.GetComponent<CanvasGroup>();
            }
        }

        private void BringTransitionTextToFront()
        {
            if (transitionText != null)
            {
                transitionText.transform.SetAsLastSibling();
            }
        }

        private void PlayTransitionMusic()
        {
            if (musicSource != null && musicSource.clip != null)
            {
                musicSource.Play();
            }
        }

        private void StopTransitionMusic()
        {
            if (musicSource != null && musicSource.isPlaying)
            {
                musicSource.Stop();
            }
        }
    }
}
