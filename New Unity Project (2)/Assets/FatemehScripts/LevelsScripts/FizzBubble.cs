using UnityEngine;

// Little soda bubbles that pop out of the doogh while CO2 is active.
public class FizzBubble : MonoBehaviour
{
    static Sprite sprite;
    SpriteRenderer sr;
    float t, life, sway;
    Vector2 vel;

    public static void At(Vector3 pos)
    {
        if (sprite == null) sprite = MakeSprite();
        var go = new GameObject("Fizz");
        go.transform.position = pos;
        float s = Random.Range(0.12f, 0.3f);
        go.transform.localScale = Vector3.one * s;
        var r = go.AddComponent<SpriteRenderer>();
        r.sprite = sprite;
        r.sortingOrder = 7;
        var b = go.AddComponent<FizzBubble>();
        b.sr = r;
        b.life = Random.Range(0.4f, 0.7f);
        b.sway = Random.Range(0f, 6f);
        b.vel = new Vector2(Random.Range(-1.2f, 1.2f), Random.Range(2.5f, 4.5f));
    }

    void Update()
    {
        t += Time.deltaTime;
        if (t >= life) { Destroy(gameObject); return; }
        transform.position += (Vector3)(vel + new Vector2(Mathf.Sin(t * 12f + sway) * 0.6f, 0f)) * Time.deltaTime;
        sr.color = new Color(1f, 1f, 1f, 1f - t / life);
    }

    // white ring bubble with a dark outline
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
                Color c = new Color(0f, 0f, 0f, 0f);
                if (r < 0.95f) c = new Color(0.9f, 0.97f, 1f, 0.55f);
                if (r >= 0.78f && r < 0.95f) c = new Color(0.08f, 0.08f, 0.12f, 1f);
                if (dx < -0.2f && dy > 0.2f && r > 0.45f && r < 0.65f) c = Color.white;
                px[y * N + x] = c;
            }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), N);
    }
}
