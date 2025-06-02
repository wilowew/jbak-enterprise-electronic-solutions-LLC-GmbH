using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelMenu : MonoBehaviour
{
    [Header("UI")]
    public RectTransform levelsContainer;         // Родитель всех кнопок
    public RectTransform[] levelButtons;          // Список кнопок уровней
    public Button leftButton;
    public Button rightButton;

    [Header("Scroll Settings")]
    public float scrollSpeed = 10f;

    private int currentIndex = 0;
    private bool isMoving = false;

    void Start()
    {
        UpdateButtons();
        SnapToLevel(0); // Позиционируемся на первом уровне
    }

    public void MoveLeft()
    {
        if (currentIndex > 0 && !isMoving)
        {
            currentIndex--;
            StartCoroutine(MoveToLevel(currentIndex));
        }
    }

    public void MoveRight()
    {
        if (currentIndex < levelButtons.Length - 1 && !isMoving)
        {
            currentIndex++;
            StartCoroutine(MoveToLevel(currentIndex));
        }
    }

    IEnumerator MoveToLevel(int index)
    {
        isMoving = true;

        Vector3 worldTarget = levelButtons[index].position;
        Vector3 localTarget = levelsContainer.InverseTransformPoint(worldTarget);
        Vector2 targetAnchoredPos = -new Vector2(localTarget.x, localTarget.y);

        while (Vector2.Distance(levelsContainer.anchoredPosition, targetAnchoredPos) > 1f)
        {
            levelsContainer.anchoredPosition = Vector2.Lerp(levelsContainer.anchoredPosition, targetAnchoredPos, Time.deltaTime * scrollSpeed);
            yield return null;
        }

        levelsContainer.anchoredPosition = targetAnchoredPos;
        isMoving = false;
        UpdateButtons();
    }

    void SnapToLevel(int index)
    {
        Vector3 worldTarget = levelButtons[index].position;
        Vector3 localTarget = levelsContainer.InverseTransformPoint(worldTarget);
        levelsContainer.anchoredPosition = -new Vector2(localTarget.x, localTarget.y);
    }

    void UpdateButtons()
    {
        leftButton.gameObject.SetActive(currentIndex > 0);
        rightButton.gameObject.SetActive(currentIndex < levelButtons.Length - 1);
    }
}
