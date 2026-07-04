using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ObjectAudioController : MonoBehaviour
{
    [Header("音频")]
    public AudioClip appearClipA = null;     // 激活时先播放的音频
    public AudioClip loopClipB = null;       // A 播放完后循环播放的音频

    public AudioSource[] disableAudioSources;

    private AudioSource audioSource;
    private Coroutine playRoutine;
    private bool isQuitting;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
    }

    private void OnEnable()
    {
        if (disableAudioSources != null)
        {
            foreach (var disableAudioSource in disableAudioSources)
            {
                if (disableAudioSource.isPlaying)
                {
                    disableAudioSource.Stop();
                }
            }
        }
        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
        }

        playRoutine = StartCoroutine(PlayAppearThenLoop());
    }

    private IEnumerator PlayAppearThenLoop()
    {
        audioSource.Stop();
        audioSource.loop = false;

        if (appearClipA != null)
        {
            audioSource.clip = appearClipA;
            audioSource.Play();

            yield return new WaitForSeconds(appearClipA.length);
        }

        if (loopClipB != null)
        {
            audioSource.clip = loopClipB;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    private void OnDisable()
    {
        if (isQuitting) return;

        if (playRoutine != null)
        {
            StopCoroutine(playRoutine);
            playRoutine = null;
        }

        if (audioSource != null)
        {
            audioSource.Stop();
        }

        if (disableAudioSources != null)
        {
            foreach (var disableAudioSource in disableAudioSources)
            {
                if (!disableAudioSource.isPlaying)
                {
                    disableAudioSource.Play();
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }
}