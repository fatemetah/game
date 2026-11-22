using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private Rigidbody2D rb1;
    int value = 1;

    void Start()
    {
        // Coins disappear 3 seconds after landing if not picked up
        if (GetComponent<TimedDespawn>() == null) gameObject.AddComponent<TimedDespawn>().lifetime = 3f;
        float rand = Random.Range(0, 2) * 2 - 1;
        rb1 = GetComponent<Rigidbody2D>();
        rb1.AddForce(new Vector2(9.8f * 25f * rand, 9.8f * 50f));
    }

    bool landed;
    static Collider2D dooghCol;

    static Collider2D DooghCollider()
    {
        if (dooghCol == null)
        {
            GameObject s = GameObject.FindWithTag("Shooter");
            if (s != null) dooghCol = s.GetComponent<Collider2D>();
            if (s != null && dooghCol == null) dooghCol = s.GetComponentInChildren<Collider2D>();
        }
        return dooghCol;
    }

    // Extra safety: once on the ground, the doogh only has to pass over the coin to take it
    void Update()
    {
        if (!landed) return;
        Collider2D d = DooghCollider();
        if (d == null || !d.gameObject.activeInHierarchy) return;
        Bounds b = d.bounds;
        if (Mathf.Abs(transform.position.x - b.center.x) < b.extents.x * 0.8f) Collect();
    }

    void Collect()
    {
        if (collected) return;
        collected = true;
        Destroy(gameObject);
        Coin_Counter.Collect(value);
        Sfx.Play("coin", 0.8f);
    }
    bool collected;

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Shooter"))
        {
            Collect();
            return;
        }
        if (coll.gameObject.CompareTag("Ground") && !landed)
        {
            landed = true;
            gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY;
            Vector3 p = transform.position; p.y = -2.9f; transform.position = p;   // lie on the floor
        }
    }
}
