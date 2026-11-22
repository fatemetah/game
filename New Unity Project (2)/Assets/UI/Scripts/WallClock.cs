using System;
using UnityEngine;

// Draws real hour/minute hands on the clock that is painted on the kitchen wall,
// showing the phone's own time. Sits on the background, so it follows the picture
// wherever FitBackgroundToCamera scales/moves it.
[RequireComponent(typeof(SpriteRenderer))]
public class WallClock : MonoBehaviour
{
    // Clock centre in the kitchen picture, in pixels from the top-left corner
    public Vector2 centerPx = new Vector2(170f, 330f);
    public float hourLengthPx = 34f;
    public float minuteLengthPx = 50f;
    public float handWidthPx = 8f;
    public Color handColor = new Color(0.24f, 0.2f, 0.18f);
    public Color dotColor = new Color(0.9f, 0.35f, 0.27f);

    Transform hourHand, minuteHand;

    void Start()
    {
        if (DooghBackgrounds.Inventory.Current != "Classic") { enabled = false; return; }
        var bg = GetComponent<SpriteRenderer>();
        if (bg.sprite == null) return;

        Bounds b = bg.sprite.bounds;                       // local units of the picture
        Vector2 px = new Vector2(bg.sprite.rect.width, bg.sprite.rect.height);
        float unit = b.size.x / px.x;                      // local units per picture pixel
        Vector3 center = new Vector3(b.min.x + centerPx.x * unit, b.max.y - centerPx.y * unit, 0f);

        Sprite bar = MakeSprite(4, 64, false);
        Sprite dot = MakeSprite(32, 32, true);

        hourHand = MakeHand("Hour Hand", bar, center, hourLengthPx * unit, handWidthPx * unit, bg, 1, handColor);
        minuteHand = MakeHand("Minute Hand", bar, center, minuteLengthPx * unit, handWidthPx * 0.75f * unit, bg, 2, handColor);
        MakeHand("Clock Dot", dot, center, 14f * unit, 14f * unit, bg, 3, dotColor);

        UpdateHands();
    }

    void Update() { UpdateHands(); }

    void UpdateHands()
    {
        if (hourHand == null) return;
        DateTime now = DateTime.Now;                       // the phone's own clock
        float minutes = now.Minute + now.Second / 60f;
        float hours = (now.Hour % 12) + minutes / 60f;
        minuteHand.localRotation = Quaternion.Euler(0, 0, -minutes * 6f);
        hourHand.localRotation = Quaternion.Euler(0, 0, -hours * 30f);
    }

    Transform MakeHand(string name, Sprite s, Vector3 center, float length, float width, SpriteRenderer bg, int order, Color c)
    {
        var go = new GameObject(name);
        go.transform.SetParent(transform, false);
        go.transform.localPosition = center + new Vector3(0, 0, -0.01f);
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = s;
        sr.color = c;
        sr.sortingLayerID = bg.sortingLayerID;
        sr.sortingOrder = bg.sortingOrder + order;
        Vector3 size = s.bounds.size;
        go.transform.localScale = new Vector3(width / size.x, length / size.y, 1f);
        return go.transform;
    }

    // A white bar with its pivot at the bottom (so it turns around the clock centre), or a round dot
    static Sprite MakeSprite(int w, int h, bool round)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Clamp;
        var px = new Color32[w * h];
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                byte a = 255;
                if (round)
                {
                    float dx = (x + 0.5f) / w - 0.5f, dy = (y + 0.5f) / h - 0.5f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    a = (byte)(255 * Mathf.Clamp01((0.5f - dist) * w));
                }
                px[y * w + x] = new Color32(255, 255, 255, a);
            }
        tex.SetPixels32(px);
        tex.Apply();
        Vector2 pivot = round ? new Vector2(0.5f, 0.5f) : new Vector2(0.5f, 0.08f);
        // pixelsPerUnit = height, so the sprite is exactly 1 unit tall (and w/h wide; the bar's width is set by scale)
        var sp = Sprite.Create(tex, new Rect(0, 0, w, h), pivot, h);
        return sp;
    }
}
