using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeInObject : MonoBehaviour
{
    [Header("��������� ���������")]
    [Tooltip("�������� ����� ������� ��������� (�������)")]
    public float delayBeforeStart = 0f;

    [Tooltip("����� ��������� (�������). ���� 0 - �������� �����")]
    public float fadeDuration = 1f;

    [Tooltip("�������� �������� ������������ (0-1)")]
    [Range(0f, 1f)]
    public float targetAlpha = 1f;

    private SpriteRenderer spriteRenderer;
    private Image image;
    private Text text;

    void Start()
    {
        // �������� ���������� ������ � �������� �������
        spriteRenderer = GetComponent<SpriteRenderer>();
        image = GetComponent<Image>();
        text = GetComponent<Text>();

        // �������� ������� ���������
        StartCoroutine(FadeInCoroutine());
    }

    IEnumerator FadeInCoroutine()
    {
        // ���� ��������� �������� ����� �������
        if (delayBeforeStart > 0)
            yield return new WaitForSeconds(delayBeforeStart);

        // ���� ����� ��������� 0 - ������������� �������� ������������ �����
        if (fadeDuration <= 0)
        {
            SetAlpha(targetAlpha);
        }
        else
        {
            // ������� ��������� ������������
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

            // ��������, ��� �������� ��������� ��������
            SetAlpha(targetAlpha);
        }
    }

    float GetCurrentAlpha()
    {
        // �������� ������� ������������ � ����������� �� ���� ����������
        if (spriteRenderer != null) return spriteRenderer.color.a;
        if (image != null) return image.color.a;
        if (text != null) return text.color.a;
        return 0f;
    }

    void SetAlpha(float alpha)
    {
        // ������������� ������������ ��� ���������� ����������
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