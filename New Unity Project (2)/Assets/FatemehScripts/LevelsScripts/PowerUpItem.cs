using System.Collections;
using UnityEngine;

// A falling, bouncing prize (MINT or CO2). Bullets go through it - touch it with the doogh.
// Disappears 7 seconds after it first lands (blinking for the last 2).
public class PowerUpItem : MonoBehaviour
{
    public enum Kind { Mint, Gas }
    public Kind kind;

    bool landed;
    Collider2D ground;
    SpriteRenderer sr;

    public static PowerUpItem Create(Kind kind, Vector3 pos)
    {
        var go = new GameObject(kind + " PowerUp");
        go.transform.position = pos;
        go.transform.localScale = Vector3.one * 0.5f;
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Resources.Load<Sprite>("PowerUps/" + kind);
        sr.sortingOrder = 6;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.gravityScale = 0.35f;
        rb.freezeRotation = true;
        rb.velocity = new Vector2(Random.value < 0.5f ? -1.2f : 1.2f, 1.5f);

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = sr.sprite != null ? sr.sprite.bounds.extents.x * 0.85f : 1f;

        go.AddComponent<StayInPlayfield>().useCeiling = true;

        var item = go.AddComponent<PowerUpItem>();
        item.kind = kind;
        item.sr = sr;
        return item;
    }

    void Start()
    {
        StartCoroutine(SafetyTimer());
    }

    void Update()
    {
        // a little wobble so it looks alive
        transform.rotation = Quaternion.Euler(0, 0, Mathf.Sin(Time.time * 4f) * 10f);
    }

    void FixedUpdate()
    {
        if (landed) return;
        if (ground == null)
        {
            GameObject g = GameObject.FindWithTag("Ground");
            if (g != null) ground = g.GetComponent<Collider2D>();
            if (ground == null) return;
        }
        var c = GetComponent<Collider2D>();
        if (c != null && c.bounds.min.y <= ground.bounds.max.y + 0.05f) StartCountdown();
    }

    void StartCountdown()
    {
        if (landed) return;
        landed = true;
        StartCoroutine(Lifetime());
    }

    IEnumerator SafetyTimer()
    {
        yield return new WaitForSeconds(30f);
        StartCountdown();
    }

    IEnumerator Lifetime()
    {
        yield return new WaitForSeconds(5f);
        float end = Time.time + 2f;
        bool on = true;
        while (Time.time < end)
        {
            on = !on;
            if (sr != null) sr.enabled = on;
            yield return new WaitForSeconds(Time.time > end - 0.8f ? 0.07f : 0.15f);
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Shooter")) return;   // bullets and everything else pass through
        if (kind == Kind.Mint) PowerUps.Mint(); else PowerUps.Gas();
        Destroy(gameObject);
    }
}
