using UnityEngine;

// A small white/yellow burst where a bullet hits. It shows up on every colour,
// including the black can (where a red flash is hard to see).
public class HitSpark : MonoBehaviour
{
    static Sprite sprite;
    SpriteRenderer sr;
    float t;
    const float Life = 0.14f;

    public static void At(Vector3 pos, SpriteRenderer over)
    {
        if (sprite == null) sprite = MakeSprite();
        var go = new GameObject("Hit Spark");
        go.transform.position = new Vector3(pos.x, pos.y, pos.z - 0.1f);
        go.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0f, 90f));
        var s = go.AddComponent<SpriteRenderer>();
        s.sprite = sprite;
        if (over != null) { s.sortingLayerID = over.sortingLayerID; s.sortingOrder = over.sortingOrder + 10; }
        go.AddComponent<HitSpark>().sr = s;
    }

    void Update()
    {
        t += Time.deltaTime;
        float k = t / Life;
        if (k >= 1f) { Destroy(gameObject); return; }
        transform.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.1f, k);
        sr.color = new Color(1f, 1f, 1f, 1f - k);
    }

    // 8-point star: white centre, yellow tips
    static Sprite MakeSprite()
    {
        const int N = 64;
        var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
        var px = new Color32[N * N];
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float dx = (x + 0.5f) / N * 2f - 1f, dy = (y + 0.5f) / N * 2f - 1f;
                float r = Mathf.Sqrt(dx * dx + dy * dy);
                float ang = Mathf.Atan2(dy, dx);
                float spikes = 0.35f + 0.65f * Mathf.Pow(Mathf.Abs(Mathf.Cos(ang * 4f)), 6f);
                float a = Mathf.Clamp01((spikes - r) * 8f);
                Color c = Color.Lerp(Color.white, new Color(1f, 0.85f, 0.2f), Mathf.Clamp01(r * 1.6f));
                px[y * N + x] = new Color(c.r, c.g, c.b, a);
            }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), N / 0.9f);   // about 0.9 units wide
    }
}
