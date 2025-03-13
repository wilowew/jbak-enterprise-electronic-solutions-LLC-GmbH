using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    public Texture2D gameCursor; // Курсор для обычного режима
    public Texture2D pauseCursor; // Курсор для паузы
    public Vector2 hotspot = Vector2.zero;

    void Start()
    {
        SetGameCursor(); // Устанавливаем игровой курсор при старте
    }

    public void SetGameCursor()
    {
        Cursor.SetCursor(gameCursor, hotspot, CursorMode.Auto);
    }

    public void SetPauseCursor()
    {
        Cursor.SetCursor(pauseCursor, hotspot, CursorMode.Auto);
    }
}

