using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    public Texture2D gameCursor; // ������ ��� �������� ������
    public Texture2D pauseCursor; // ������ ��� �����
    public Vector2 hotspot = Vector2.zero;

    void Start()
    {
        SetGameCursor(); // ������������� ������� ������ ��� ������
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

