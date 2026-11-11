using UnityEngine;

// Bouncing off the walls and the floor.
// Floor: the first bounce goes up to about half the height of the screen,
// every next bounce reaches 3/4 of the height of the one before,
// but never less than MinHeight, so cans and sugar keep hopping instead of
// lying still in the doogh's way.
// The height is worked out from the gravity, so it is the same in every game mode
// (and with MINT, things just move slower along the same path).
public class Enemy_Bounce : MonoBehaviour
{
    const float FirstBounceShare = 0.5f;   // first bounce: half of the play area
    const float Decay = 0.75f;             // each bounce: three quarters of the previous one
    const float MinHeight = 1.5f;          // world units (about one sugar cube)

    Rigidbody2D rb;
    Vector3 lastVelocity;
    float nextHeight = -1f;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        // count it only once, so each bounce really is 3/4 of the one before
        if (Time.time - lastBounceTime < 0.25f) return lastBounceSpeed;
        lastBounceTime = Time.time;

        if (nextHeight < 0f) nextHeight = FirstBounceShare * PlayHeight();
        float h = Mathf.Max(nextHeight, MinHeight);
        nextHeight = h * Decay;
        float g = Mathf.Abs(Physics2D.gravity.y) * Mathf.Max(0.0001f, rb.gravityScale);
        lastBounceSpeed = Mathf.Sqrt(2f * g * h);   // speed that reaches exactly height h
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
