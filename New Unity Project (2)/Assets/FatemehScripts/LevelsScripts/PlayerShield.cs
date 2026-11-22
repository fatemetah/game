using UnityEngine;

// Added to the player when they catch the ghost.
// While active, cans can't kill the player. The player looks see-through (ghostly)
// and blinks during the last 2 seconds so you know it is about to end.
public class PlayerShield : MonoBehaviour
{
    float endTime = -1f;
    SpriteRenderer[] sprites;
    Color[] baseColors;

    public float SecondsLeft => Mathf.Max(0f, endTime - Time.time);

    public bool IsActive { get { return Time.time < endTime; } }

    public void Activate(float seconds)
    {
        endTime = Time.time + seconds;
        if (sprites == null)
        {
            sprites = GetComponentsInChildren<SpriteRenderer>(true);
            baseColors = new Color[sprites.Length];
            for (int i = 0; i < sprites.Length; i++) baseColors[i] = sprites[i].color;
        }
    }

    void Update()
    {
        if (sprites == null) return;

        float left = endTime - Time.time;
        if (left <= 0f)
        {
            SetLook(1f, false);
            sprites = null;
            return;
        }

        bool blinkOff = left < 2f && Mathf.Repeat(Time.time * 8f, 1f) < 0.5f;
        SetLook(blinkOff ? 0.2f : 0.55f, true);
    }

    void SetLook(float alpha, bool tint)
    {
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null) continue;
            Color c = baseColors[i];
            if (tint) c = Color.Lerp(c, new Color(0.6f, 0.9f, 1f), 0.35f);
            c.a = baseColors[i].a * alpha;
            sprites[i].color = c;
        }
    }
}
