using UnityEngine;

// Keeps an object inside the play area (between LeftWall, RightWall and Ground).
// If it ever slips past a wall (for example in a corner), it is pushed back in
// and bounces off in the other direction with the same speed.
[RequireComponent(typeof(Rigidbody2D))]
public class StayInPlayfield : MonoBehaviour
{
    public bool useCeiling = false;   // also bounce off the top of the screen

    Rigidbody2D rb;
    bool insideTop;   // things spawn above the screen: the ceiling only counts once they have come in
    Collider2D[] colliders;
    float minX = float.NegativeInfinity, maxX = float.PositiveInfinity;
    float minY = float.NegativeInfinity, maxY = float.PositiveInfinity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponents<Collider2D>();

        Bounds b;
        if (WallBounds("LeftWall", out b)) minX = b.max.x;
        if (WallBounds("RightWall", out b)) maxX = b.min.x;
        if (WallBounds("Ground", out b)) minY = b.max.y;

        Camera cam = Camera.main;
        if (useCeiling && cam != null && cam.orthographic)
            maxY = cam.transform.position.y + cam.orthographicSize;
    }

    static bool WallBounds(string tag, out Bounds bounds)
    {
        bounds = new Bounds();
        GameObject wall = GameObject.FindWithTag(tag);
        if (wall == null) return false;
        Collider2D c = wall.GetComponent<Collider2D>();
        if (c == null) return false;
        bounds = c.bounds;
        return true;
    }

    void FixedUpdate()
    {
        Bounds me;
        if (!MyBounds(out me)) return;

        Vector2 pos = rb.position;
        Vector2 vel = rb.velocity;
        bool moved = false;

        if (me.min.x < minX) { pos.x += minX - me.min.x; vel.x = Mathf.Abs(vel.x); moved = true; }
        else if (me.max.x > maxX) { pos.x -= me.max.x - maxX; vel.x = -Mathf.Abs(vel.x); moved = true; }

        if (me.min.y < minY)
        {
            pos.y += minY - me.min.y;
            // cans and sugar use the same shrinking bounce as a normal floor hit
            var bounce = GetComponent<Enemy_Bounce>();
            vel.y = (bounce != null && vel.y < 0f) ? bounce.NextBounceSpeed() : Mathf.Abs(vel.y);
            moved = true;
        }
        if (me.max.y <= maxY) insideTop = true;
        else if (insideTop) { pos.y -= me.max.y - maxY; vel.y = -Mathf.Abs(vel.y); moved = true; }

        if (moved)
        {
            rb.position = pos;
            rb.velocity = vel;
        }
    }

    bool MyBounds(out Bounds result)
    {
        result = new Bounds();
        bool any = false;
        foreach (Collider2D c in colliders)
        {
            if (c == null || !c.enabled) continue;
            if (!any) { result = c.bounds; any = true; }
            else result.Encapsulate(c.bounds);
        }
        return any;
    }
}
