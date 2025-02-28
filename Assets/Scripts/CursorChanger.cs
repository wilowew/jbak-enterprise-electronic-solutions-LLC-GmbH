using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    public Texture2D customCursor; 
    public Vector2 hotspot = Vector2.zero; 

    void Start()
    {
        Cursor.SetCursor(customCursor, hotspot, CursorMode.Auto);
    }
}

