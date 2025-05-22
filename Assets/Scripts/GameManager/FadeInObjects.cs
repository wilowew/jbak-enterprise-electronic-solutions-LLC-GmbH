using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInObject : MonoBehaviour
{
    [Header("Настройки появления")]
    [Tooltip("Задержка перед началом появления (секунды)")]
    public float delayBeforeStart = 0f;

    [Tooltip("Время появления (секунды). Если 0 - появится резко")]
    public float fadeDuration = 1f;

    [Tooltip("Конечное значение прозрачности (0-1)")]
    [Range(0f, 1f)]
    public float targetAlpha = 1f;

    private SpriteRenderer spriteRenderer;
    private Image image;
    private Text text;

    void Start()
    {
        // Получаем компоненты только с текущего объекта
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
        text = GetComponent<Text>();

        // Начинаем процесс появления
        StartCoroutine(FadeInCoroutine());
    }

    IEnumerator FadeInCoroutine()
    {
        // Ждем указанную задержку перед началом
        if (delayBeforeStart > 0)
            yield return new WaitForSeconds(delayBeforeStart);

        // Если время появления 0 - устанавливаем конечную прозрачность сразу
        if (fadeDuration <= 0)
        {
            SetAlpha(targetAlpha);
        }
        else
        {
            // Плавное изменение прозрачности
            float timer = 0f;
            float startAlpha = GetCurrentAlpha();

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                float progress = Mathf.Clamp01(timer / fadeDuration);
                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, progress);

                SetAlpha(currentAlpha);
                yield return null;
            }

            // Убедимся, что достигли конечного значения
            SetAlpha(targetAlpha);
        }
    }

    float GetCurrentAlpha()
    {
        // Получаем текущую прозрачность в зависимости от типа компонента
        if (spriteRenderer != null) return spriteRenderer.color.a;
        if (image != null) return image.color.a;
        if (text != null) return text.color.a;
        return 0f;
    }

    void SetAlpha(float alpha)
    {
        // Устанавливаем прозрачность для имеющегося компонента
        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        if (image != null)
        {
            Color color = image.color;
            color.a = alpha;
            image.color = color;
        }

        if (text != null)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }
}