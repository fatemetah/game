//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public float speed = 3f;
    private Rigidbody2D rb;
    public GameObject NextModel;
    public float health = 2f;

     void Start()
    {
        float turni = Random.Range(0, 2) * 2 - 1;
        rb = GetComponent<Rigidbody2D>();

        // How many bullets each enemy takes: red can 4, black can 3, sugar cube 2
        if (gameObject.CompareTag("Enemy3")) health = 4f;
        else if (gameObject.CompareTag("Enemy2")) health = 3f;
        else if (gameObject.CompareTag("Enemy1")) health = 2f;
        maxHealth = health;

        // Never leave the play area
        StayInPlayfield bounds = GetComponent<StayInPlayfield>();
        if (bounds == null) bounds = gameObject.AddComponent<StayInPlayfield>();

        if (gameObject.CompareTag("ghost"))
        {
            // The ghost floats around inside the screen, then disappears if nobody catches it
            bounds.useCeiling = true;
            rb.AddForce(new Vector2(9.8f * 25f * turni, 9.8f * 50f));
            StartCoroutine(GhostSafetyTimer());
            return;
        }

        rb.AddForce(new Vector2(9.8f * 25f * turni, 9.8f * 50f) * GameDifficulty.Speed);
        rb.gravityScale = GameDifficulty.Gravity;

        // About half of the cans and sugar cubes come down tilted and slowly spinning
        if (Random.value < 0.5f)
        {
            float side = Random.value < 0.5f ? -1f : 1f;
            transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(15f, 35f) * side);
            rb.angularVelocity = Random.Range(40f, 110f) * side;   // degrees per second
            rb.angularDrag = 0f;
        }
    }

    public float ghostLifetime = 7f;   // seconds the ghost stays after it first touches the ground

    bool ghostLanded;
    Collider2D groundCollider;

    // The countdown starts when the ghost touches the ground, not when it appears in the air
    void StartGhostCountdown()
    {
        if (ghostLanded) return;
        ghostLanded = true;
        StartCoroutine(GhostLifetime());
    }

    void OnCollisionEnter2D(Collision2D c)
    {
        if (gameObject.CompareTag("ghost") && c.gameObject.CompareTag("Ground")) StartGhostCountdown();
    }

    // ---- MINT: everything slows down. Speed and gravity are scaled together so the
    // path stays the same, only slower. (Skips the very first physics step so the
    // starting push is applied normally first.)
    bool slowed, firstStep = true;

    void ApplyMint()
    {
        if (rb == null) return;
        if (firstStep) { firstStep = false; return; }
        bool want = PowerUps.MintActive;
        if (want == slowed) return;
        float f = PowerUps.SlowFactor;
        if (want) { rb.velocity *= f; rb.angularVelocity *= f; rb.gravityScale *= f * f; }
        else { rb.velocity /= f; rb.angularVelocity /= f; rb.gravityScale /= f * f; }
        slowed = want;
    }

    void FixedUpdate()
    {
        ApplyMint();
        if (!gameObject.CompareTag("ghost") || ghostLanded) return;
        if (groundCollider == null)
        {
            GameObject g = GameObject.FindWithTag("Ground");
            if (g != null) groundCollider = g.GetComponent<Collider2D>();
            if (groundCollider == null) return;
        }
        // The playfield keeper bounces it off the ground without a physical hit, so check the height too
        foreach (Collider2D col in GetComponents<Collider2D>())
            if (col.enabled && col.bounds.min.y <= groundCollider.bounds.max.y + 0.05f) { StartGhostCountdown(); return; }
    }

    // Just in case it never reaches the ground (stuck somewhere): remove it after 30 s anyway
    IEnumerator GhostSafetyTimer()
    {
        yield return new WaitForSeconds(30f);
        StartGhostCountdown();
    }

    IEnumerator GhostLifetime()
    {
        // Visible for a while, blinks for the last 2 seconds, then is removed
        yield return new WaitForSeconds(Mathf.Max(0f, ghostLifetime - 2f));
        SpriteRenderer[] sprites = GetComponentsInChildren<SpriteRenderer>();
        float end = Time.time + 2f;
        bool on = true;
        while (Time.time < end)
        {
            on = !on;
            foreach (SpriteRenderer s in sprites) s.enabled = on;
            yield return new WaitForSeconds(Time.time > end - 0.8f ? 0.07f : 0.15f);
        }
        Destroy(gameObject);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        bool isGhost = gameObject.CompareTag("ghost");
        bool isCan = gameObject.CompareTag("Enemy1") || gameObject.CompareTag("Enemy2") || gameObject.CompareTag("Enemy3");

        if (other.gameObject.CompareTag("Shooter"))
        {
            if (isGhost)
            {
                // Ghost is a prize: catching it makes the player safe for 10 seconds
                PlayerShield shield = other.GetComponent<PlayerShield>();
                if (shield == null) shield = other.gameObject.AddComponent<PlayerShield>();
                shield.Activate(10f);
                Sfx.Play("ghost", 1f, 0f);
                Destroy(gameObject);
            }
            else if (isCan)
            {
                PlayerShield shield = other.GetComponent<PlayerShield>();
                if (shield == null || !shield.IsActive)
                {
                    other.gameObject.SetActive(false);
                }
            }
            return;
        }

        // Bullets fly through the ghost: only touching it with the bottle catches it
        if (isGhost) return;

        if (other.gameObject.CompareTag("Bullet") && isCan)
        {
            if (health <= 0f) return;   // already destroyed this frame
            // A bullet has two colliders, so it can "touch" twice at once. Count each bullet only once:
            // the first touch uses it up (turns it off), any later touch from it is ignored.
            if (!other.gameObject.activeSelf) return;
            Bullet2.Recycle(other.gameObject);
            health -= 1f;
            if (health > 0f)
            {
                // still alive: show how damaged it is, shake, flash and a little spark where it was hit
                ShowDamage(other.transform.position);
                return;
            }
            if (health <= 0f)
            {
                // red can: deep boom, black can: normal pop, sugar cube: thin tink
                if (gameObject.CompareTag("Enemy3")) Sfx.Play("can_big", 1f);
                else if (gameObject.CompareTag("Enemy2")) Sfx.Play("can_small", 0.9f);
                else Sfx.Play("sugar", 0.8f);
                Destroy(gameObject);
                if (gameObject.CompareTag("Enemy1"))
                {
                    NextLevel();
                }
                if (gameObject.CompareTag("Enemy2") || gameObject.CompareTag("Enemy3"))
                {
                    NextLevel2();
                }
            }
        }
    }

    // ---- damage look: 3 stages for the cans (dented, squeezed + cracked, crushed + leaking)
    float maxHealth = 1f;
    SpriteRenderer body;
    Color bodyColor = Color.white;

    void ShowDamage(Vector3 hitPoint)
    {
        HitSpark.At(hitPoint, body != null ? body : GetComponent<SpriteRenderer>());
        Sfx.Play("hit", 0.55f, 0.12f);
        if (body == null) { body = GetComponent<SpriteRenderer>(); if (body != null) bodyColor = body.color; }

        string kind = gameObject.CompareTag("Enemy3") ? "Red" : gameObject.CompareTag("Enemy2") ? "Black" : "Sugar";
        if (body != null)
        {
            string file;
            if (kind == "Sugar") file = "Sugar1";   // the sugar cube just looks sad after its first hit
            else
            {
                float lost = (maxHealth - health) / maxHealth;            // 0..1
                int stage = Mathf.Clamp(Mathf.CeilToInt(lost * 3f), 1, 3); // red 4 hits: 1,2,3   black 3 hits: 1,2
                file = kind + stage;
            }
            Sprite s = Resources.Load<Sprite>("Damage/" + file);
            if (s != null) body.sprite = s;
        }
        StopCoroutine("HitShake");
        StartCoroutine("HitShake");
    }

    Vector3 originalScale;
    bool scaleSaved;

    IEnumerator HitShake()
    {
        if (!scaleSaved) { originalScale = transform.localScale; scaleSaved = true; }
        float t = 0f;
        while (t < 0.15f)
        {
            t += Time.deltaTime;
            float k = 1f - t / 0.15f;
            transform.localScale = originalScale * (1f + 0.12f * k);
            if (body != null) body.color = Color.Lerp(bodyColor, new Color(1f, 0.55f, 0.55f, bodyColor.a), k);
            yield return null;
        }
        transform.localScale = originalScale;
        if (body != null) body.color = bodyColor;
    }

     void NextLevel()
    {
        Vector3 position = transform.position;
        GameObject coin = Instantiate(NextModel, position, Quaternion.identity);
        coin.SetActive(true);
    }
    void NextLevel2()
    {

        Vector3 position = transform.position;
        GameObject coin = Instantiate(NextModel, position, Quaternion.identity);
        GameObject coin1 = Instantiate(NextModel, position, Quaternion.identity);
        coin.SetActive(true);
        coin1.SetActive(true);
    }
}




