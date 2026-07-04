using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Arendalle
{
    [DisallowMultipleComponent]
    public sealed class ButtonClickSound : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private AudioClip clickAudioClip;
        [SerializeField] private AudioSource audioSource;
        [SerializeField, Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private bool requireInteractable = true;
        [SerializeField] private bool leftMouseButtonOnly = true;

        public void OnPointerDown(PointerEventData eventData)
        {
            if (leftMouseButtonOnly && eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }

            if (requireInteractable)
            {
                Selectable selectable = GetComponent<Selectable>();
                if (selectable != null && !selectable.IsInteractable())
                {
                    return;
                }
            }

            Play();
        }

        public void Play()
        {
            if (clickAudioClip == null)
            {
                return;
            }

            AudioSource source = ResolveAudioSource();
            if (source != null)
            {
                source.PlayOneShot(clickAudioClip, volume);
            }
        }

        private AudioSource ResolveAudioSource()
        {
            if (audioSource != null)
            {
                return audioSource;
            }

            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.loop = false;
                audioSource.spatialBlend = 0f;
            }

            return audioSource;
        }
    }
}
