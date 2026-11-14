using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomCursor : MonoBehaviour
{
    public Texture2D cursorTexture;
    public Vector2 hotspot = Vector2.zero;

    void Start()
    {
        UnityEngine.Cursor.SetCursor(cursorTexture, hotspot, UnityEngine.CursorMode.Auto);
    }

}
