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

    [Header("Enemy Tracking")]
    public bool IsTrackingEnemy = false;

    [Tooltip("Сколько врагов нужно убить для смены музыки")]
    public int enemyKillThreshold = 5;

    [Tooltip("Новый трек, который включается при достижении лимита")]
    public AudioClip newMusicClip;

    [Tooltip("С какой секунды начинать новый трек")]
    public float newTrackStartTime = 0f;

    [Tooltip("Длительность затухания и появления")]
    public float fadeDuration = 2f;

    private int killCount = 0;
    private bool hasSwitchedMusic = false;

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
        StartCoroutine(FadeIn(fadeDuration));
    }

    void OnDestroy()
    {
        if (audioSource.isPlaying)
        {
            fadeOutCoroutine = StartCoroutine(FadeOutAndStop(fadeDuration));
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

    public void AddKillPoint()
    {
        if (!IsTrackingEnemy || hasSwitchedMusic) return;

        killCount++;

        if (killCount >= enemyKillThreshold && newMusicClip != null)
        {
            hasSwitchedMusic = true;
            StartCoroutine(SwitchToNewTrack());
        }
    }

    private IEnumerator SwitchToNewTrack()
    {
        yield return StartCoroutine(FadeOutAndStop(fadeDuration));

        audioSource.clip = newMusicClip;

        if (newTrackStartTime > 0f && newTrackStartTime < newMusicClip.length)
        {
            audioSource.time = newTrackStartTime;
        }
        else
        {
            audioSource.time = 0f;
        }

        yield return null; // <<< ДОБАВЛЕНА задержка

        audioSource.Play();
        yield return StartCoroutine(FadeIn(fadeDuration));
    }
}
