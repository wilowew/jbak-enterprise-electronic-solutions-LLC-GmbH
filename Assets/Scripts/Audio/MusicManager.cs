using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class BackgroundMusic : MonoBehaviour
{
    private AudioSource audioSource;
    private Coroutine fadeOutCoroutine;
    private float originalVolume;

    [Tooltip("С какой секунды начинать воспроизведение (опционально)")]
    public float startTime = 0f;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        originalVolume = audioSource.volume;
        audioSource.loop = true;
        audioSource.playOnAwake = false;
    }

    void Start()
    {
        if (startTime > 0f && startTime < audioSource.clip.length)
        {
            audioSource.time = startTime;
        }

        audioSource.volume = 0f;
        audioSource.Play();
        StartCoroutine(FadeIn(2f));
    }

    void OnDestroy()
    {
        if (audioSource.isPlaying)
        {
            fadeOutCoroutine = StartCoroutine(FadeOutAndStop(2f));
        }
    }

    IEnumerator FadeIn(float duration)
    {
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(0f, originalVolume, t / duration);
            yield return null;
        }

        audioSource.volume = originalVolume;
    }

    IEnumerator FadeOutAndStop(float duration)
    {
        float startVolume = audioSource.volume;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, 0f, t / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = originalVolume;
    }
}