using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Shows what a prize is doing:
//   - an icy frame around the screen while MINT is on
//   - a small timer (icon + seconds left) under the score for each active prize
public class PowerUpHud : MonoBehaviour
{
    static PowerUpHud inst;

    Image frost, tint;
    GameObject mintChip, gasChip, ghostChip;
    TMP_Text mintText, gasText, ghostText;
    PlayerShield shield;
    float frostAlpha;

    public static void Ensure()
    {
        if (inst == null) inst = new GameObject("PowerUp HUD").AddComponent<PowerUpHud>();
    }

    public static void ShowShield(PlayerShield activeShield, Sprite icon)
    {
        Ensure();
        inst.shield = activeShield;
        if (inst.ghostChip == null)
            inst.ghostChip = inst.Chip("Ghost Timer", icon, inst.mintText.font, out inst.ghostText);
    }

    void Awake()
    {
        var canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 4;   // under the pause button, pause menu and game over card
        var scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(900, 1600);
        scaler.matchWidthOrHeight = 0f;

        tint = FullScreen("Ice Tint", null, new Color(0.55f, 0.85f, 1f, 0f));
        frost = FullScreen("Frost", FrostSprite(), new Color(1f, 1f, 1f, 0f));

        TMP_FontAsset font = null;
        counter = FindObjectOfType<Coin_Counter>();
        if (counter != null && counter.label != null) font = counter.label.font;

        mintChip = Chip("Mint Timer", Resources.Load<Sprite>("PowerUps/Mint"), font, out mintText);
        gasChip = Chip("Gas Timer", Resources.Load<Sprite>("PowerUps/Gas"), font, out gasText);
    }

    Image FullScreen(string name, Sprite s, Color c)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.sprite = s;
        img.color = c;
        img.raycastTarget = false;
        return img;
    }

    GameObject Chip(string name, Sprite icon, TMP_FontAsset font, out TMP_Text label)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.sizeDelta = new Vector2(190f, 86f);
        var bg = go.GetComponent<Image>();
        bg.sprite = PillSprite();
        bg.color = new Color(0.08f, 0.08f, 0.12f, 0.85f);
        bg.raycastTarget = false;

        var ic = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        ic.transform.SetParent(go.transform, false);
        var irt = (RectTransform)ic.transform;
        irt.anchorMin = irt.anchorMax = new Vector2(0.5f, 0.5f);
        irt.sizeDelta = new Vector2(72f, 72f);
        irt.anchoredPosition = new Vector2(-48f, 0f);
        var ii = ic.GetComponent<Image>();
        ii.sprite = icon; ii.preserveAspect = true; ii.raycastTarget = false;

        var tx = new GameObject("Seconds", typeof(RectTransform), typeof(TextMeshProUGUI));
        tx.transform.SetParent(go.transform, false);
        var trt = (RectTransform)tx.transform;
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0.5f);
        trt.sizeDelta = new Vector2(100f, 80f);
        trt.anchoredPosition = new Vector2(38f, 2f);
        label = tx.GetComponent<TMP_Text>();
        if (font != null) label.font = font;
        label.fontSize = 62;
        label.alignment = TextAlignmentOptions.Center;
        label.color = new Color(1f, 0.84f, 0f);
        label.raycastTarget = false;

        go.SetActive(false);
        return go;
    }

    Coin_Counter counter;

    void Update()
    {
        bool mint = PowerUps.MintActive, gas = PowerUps.GasActive;

        // hide everything on the GAME OVER screen (the score badge is hidden there too)
        if (counter == null) counter = FindObjectOfType<Coin_Counter>(true);
        bool visible = counter != null && counter.gameObject.activeInHierarchy;
        mint &= visible;
        gas &= visible;
        bool ghost = visible && shield != null && shield.isActiveAndEnabled && shield.IsActive;

        // icy screen fades in/out
        frostAlpha = Mathf.MoveTowards(frostAlpha, mint ? 1f : 0f, Time.unscaledDeltaTime * 3f);
        frost.color = new Color(1f, 1f, 1f, frostAlpha * 0.9f);
        tint.color = new Color(0.55f, 0.85f, 1f, frostAlpha * 0.14f);

        int count = (mint ? 1 : 0) + (gas ? 1 : 0) + (ghost ? 1 : 0);
        float x = -(count - 1) * 100f;
        UpdateTimer(mintChip, mintText, mint, PowerUps.MintLeft, ref x);
        UpdateTimer(gasChip, gasText, gas, PowerUps.GasLeft, ref x);
        if (ghostChip != null)
            UpdateTimer(ghostChip, ghostText, ghost, ghost ? shield.SecondsLeft : 0f, ref x);
    }

    static void UpdateTimer(GameObject chip, TMP_Text label, bool active, float left, ref float x)
    {
        chip.SetActive(active);
        if (!active) return;
        ((RectTransform)chip.transform).anchoredPosition = new Vector2(x, -160f);
        Show(label, left);
        x += 200f;
    }

    static void Show(TMP_Text t, float left)
    {
        t.text = Mathf.CeilToInt(left).ToString();
        // blink in the last 3 seconds so you know it is ending
        bool dim = left < 3f && Mathf.Repeat(Time.time * 4f, 1f) < 0.5f;
        t.alpha = dim ? 0.35f : 1f;
    }

    // icy border: transparent in the middle, frosty white-blue towards the edges
    static Sprite FrostSprite()
    {
        const int N = 256;
        var tex = new Texture2D(N, N, TextureFormat.RGBA32, false);
        var px = new Color32[N * N];
        var rnd = new System.Random(5);
        for (int y = 0; y < N; y++)
            for (int x = 0; x < N; x++)
            {
                float dx = Mathf.Abs((x + 0.5f) / N * 2f - 1f), dy = Mathf.Abs((y + 0.5f) / N * 2f - 1f);
                float e = Mathf.Max(dx, dy * 0.92f);
                float a = Mathf.Clamp01((e - 0.62f) / 0.38f);
                a = Mathf.Pow(a, 1.4f) * 0.85f;
                if (a > 0.05f && rnd.NextDouble() < 0.06) a = Mathf.Min(1f, a + 0.35f);   // sparkly ice crystals
                px[y * N + x] = new Color(0.86f, 0.96f, 1f, a);
            }
        tex.SetPixels32(px);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, N, N), new Vector2(0.5f, 0.5f), 100f);
    }

    static Sprite pill;
    static Sprite PillSprite()
    {
        if (pill != null) return pill;
        const int W = 128, H = 64;
        var tex = new Texture2D(W, H, TextureFormat.RGBA32, false);
        var px = new Color32[W * H];
        for (int y = 0; y < H; y++)
            for (int x = 0; x < W; x++)
            {
                float r = H / 2f;
                float cx = Mathf.Clamp(x + 0.5f, r, W - r);
                float d = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), new Vector2(cx, r));
                byte a = (byte)(255 * Mathf.Clamp01(r - d));
                px[y * W + x] = new Color32(255, 255, 255, a);
            }
        tex.SetPixels32(px);
        tex.Apply();
        pill = Sprite.Create(tex, new Rect(0, 0, W, H), new Vector2(0.5f, 0.5f), 100f);
        return pill;
    }
}
