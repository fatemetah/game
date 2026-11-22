using UnityEngine;

// Sugar keeps its arcade bounce, with enough clearance to rotate at the top of its arc.
public class Enemy_Bounce : MonoBehaviour
{
    const float MinShare = 0.45f;          // a bounce always reaches at least a bit under half of the play area

    Rigidbody2D rb;
    Vector3 lastVelocity;
    BoxCollider2D box;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
    }

    void Update()
    {
        lastVelocity = rb.velocity;
    }

    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.CompareTag("LeftWall") || coll.CompareTag("RightWall") || coll.CompareTag("Ground"))
            GetComponent<BoxCollider2D>().enabled = true;
    }

    void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.CompareTag("LeftWall") || coll.gameObject.CompareTag("RightWall"))
        {
            GetComponent<BoxCollider2D>().enabled = false;
            var speed = lastVelocity.magnitude;
            var direction = Vector3.Reflect(lastVelocity.normalized, coll.contacts[0].normal);
            rb.velocity = direction * Mathf.Max(speed, 0f);
        }
        if (coll.gameObject.CompareTag("Ground"))
        {
            GetComponent<BoxCollider2D>().enabled = false;
            rb.velocity = new Vector2(lastVelocity.x, NextBounceSpeed());
        }
    }

    // Upward speed for the next floor bounce (also used by StayInPlayfield as a backup)
    float lastBounceTime = -10f, lastBounceSpeed;

    public float NextBounceSpeed()
    {
        // The same floor hit can be reported twice (the collision and the play-area guard):
        // count it only once
        if (Time.time - lastBounceTime < 0.25f) return lastBounceSpeed;
        lastBounceTime = Time.time;

        float speed = Mathf.Abs(lastVelocity.y);           // how fast it came down

        float g = Mathf.Abs(Physics2D.gravity.y) * Mathf.Max(0.0001f, rb.gravityScale);
        float ph = PlayHeight();
        float minSpeed = Mathf.Sqrt(2f * g * MinShare * ph);   // speed that reaches the lowest allowed height
        lastBounceSpeed = Mathf.Max(speed, minSpeed);
        Camera cam = Camera.main;
        if (cam != null && cam.orthographic)
        {
            float radius = 0.5f * Vector2.Scale(box.size, transform.lossyScale).magnitude;
            float top = cam.transform.position.y + cam.orthographicSize;
            float clearance = top - rb.worldCenterOfMass.y - radius - 0.15f;
            float maxSpeed = Mathf.Sqrt(2f * g * Mathf.Max(0f, clearance));
            lastBounceSpeed = Mathf.Min(lastBounceSpeed, maxSpeed);
        }
        return lastBounceSpeed;
    }

    // From the floor to the top of the screen
    static float PlayHeight()
    {
        Camera cam = Camera.main;
        GameObject ground = GameObject.FindWithTag("Ground");
        if (cam == null || ground == null) return 8f;
        Collider2D c = ground.GetComponent<Collider2D>();
        float floor = c != null ? c.bounds.max.y : ground.transform.position.y;
        float top = cam.transform.position.y + cam.orthographicSize;
        return Mathf.Max(2f, top - floor);
    }
}
