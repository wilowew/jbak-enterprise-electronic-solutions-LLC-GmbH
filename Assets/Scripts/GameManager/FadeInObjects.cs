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

    [Header("Дополнительно")]
    [Tooltip("Объект (например, кнопка), у которого нужно изменить состояние interactable")]
    public Selectable objectToToggle;

    private SpriteRenderer spriteRenderer;
    private Image image;
    private Text text;

    private bool initialInteractable;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
        text = GetComponent<Text>();

        // Сохраняем изначальное состояние
        if (objectToToggle != null)
        {
            initialInteractable = objectToToggle.interactable;
        }

        StartCoroutine(FadeInCoroutine());
    }

    IEnumerator FadeInCoroutine()
    {
        if (delayBeforeStart > 0)
            yield return new WaitForSeconds(delayBeforeStart);

        if (fadeDuration <= 0)
        {
            SetAlpha(targetAlpha);
        }
        else
        {
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

            SetAlpha(targetAlpha);
        }

        // Инвертируем состояние interactable
        if (objectToToggle != null)
        {
            objectToToggle.interactable = !initialInteractable;
        }
    }

    float GetCurrentAlpha()
    {
        if (spriteRenderer != null) return spriteRenderer.color.a;
        if (image != null) return image.color.a;
        if (text != null) return text.color.a;
        return 0f;
    }

    void SetAlpha(float alpha)
    {
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
