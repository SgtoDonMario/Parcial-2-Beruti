using UnityEngine;

public class Crosshair : MonoBehaviour
{
    public Texture2D crosshairTexture;
    public float size = 32f;

    void OnGUI()
    {
        if (crosshairTexture == null) return;

        float xMin = (Screen.width - size) / 2;
        float yMin = (Screen.height - size) / 2;
        GUI.DrawTexture(new Rect(xMin, yMin, size, size), crosshairTexture);
    }
}
