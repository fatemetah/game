using System.Collections;
using UnityEngine;

// Removes a coin a few seconds after it lands on the ground, if nobody picked it up.
// It blinks during the last second as a warning.
public class TimedDespawn : MonoBehaviour
{
    public float lifetime = 3f;   // seconds after touching the ground
    public float blinkTime = 1f;

    bool started = false;

    void OnTriggerEnter2D(Collider2D other) { CheckGround(other.gameObject); }
    void OnCollisionEnter2D(Collision2D other) { CheckGround(other.gameObject); }

    void CheckGround(GameObject other)
    {
        if (started || !other.CompareTag("Ground")) return;
        started = true;
        StartCoroutine(Countdown());
    }

    IEnumerator Countdown()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, lifetime - blinkTime));

        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        float end = Time.time + blinkTime;
        bool on = true;
        while (Time.time < end)
        {
            on = !on;
            foreach (SpriteRenderer s in sprites) if (s != null) s.enabled = on;
            yield return new WaitForSeconds(0.1f);
        }
        Destroy(gameObject);
    }
}
