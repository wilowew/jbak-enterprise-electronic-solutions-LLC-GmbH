using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;

public class ImageSequenceController : MonoBehaviour
{
    [Header("Настройки последовательности")]
    [SerializeField] private List<ImageData> images = new List<ImageData>();
    [SerializeField] private bool loopSequence = true;
    [SerializeField] private bool startOnEnable = true;

    [Header("События")]
    public UnityEvent OnSequenceStart;
    public UnityEvent OnSequenceEnd;

    private CanvasGroup[] canvasGroups;
    private Coroutine sequenceCoroutine;

    private void Awake()
    {
        InitializeCanvasGroups();
    }

    private void OnEnable()
    {
        if (startOnEnable) StartSequence();
    }

    private void InitializeCanvasGroups()
    {
        canvasGroups = new CanvasGroup[images.Count];
        for (int i = 0; i < images.Count; i++)
        {
            if (images[i].image != null)
            {
                canvasGroups[i] = images[i].image.gameObject.AddComponent<CanvasGroup>();
                canvasGroups[i].alpha = 0;
            }
        }
    }

    public void StartSequence()
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        sequenceCoroutine = StartCoroutine(ImageSequenceRoutine());
        OnSequenceStart?.Invoke();
    }

    public void StopSequence()
    {
        if (sequenceCoroutine != null) StopCoroutine(sequenceCoroutine);
        ResetAllAlphas();
        OnSequenceEnd?.Invoke();
    }

    private IEnumerator ImageSequenceRoutine()
    {
        do
        {
            for (int i = 0; i < images.Count; i++)
            {
                if (images[i].image == null) continue;

                // Ожидание перед показом
                yield return new WaitForSeconds(images[i].showDelay);

                // Анимация появления
                yield return StartCoroutine(FadeImage(canvasGroups[i], 1, images[i].fadeDuration));

                // Время видимости
                yield return new WaitForSeconds(images[i].visibleTime);

                // Анимация исчезновения
                yield return StartCoroutine(FadeImage(canvasGroups[i], 0, images[i].fadeDuration));

                // Ожидание после скрытия
                yield return new WaitForSeconds(images[i].hideDelay);
            }
        } while (loopSequence);

        OnSequenceEnd?.Invoke();
    }

    private IEnumerator FadeImage(CanvasGroup cg, float targetAlpha, float duration)
    {
        float startAlpha = cg.alpha;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
    }

    private void ResetAllAlphas()
    {
        foreach (var cg in canvasGroups)
        {
            if (cg != null) cg.alpha = 0;
        }
    }
}