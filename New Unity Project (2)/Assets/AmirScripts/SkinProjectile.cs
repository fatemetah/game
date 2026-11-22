using UnityEngine;

public sealed class SkinProjectile : MonoBehaviour
{
    Rigidbody2D body;
    SpriteRenderer visual;
    Camera playCamera;
    float expires;
    public bool IsOrangeSeed { get; private set; }

    public static SkinProjectile Create(Sprite sprite, GameObject prefab, string skin)
    {
        var go = new GameObject(skin + " shot");
        go.SetActive(false);
        go.layer = prefab.layer;
        go.tag = prefab.tag;
        var render = go.AddComponent<SpriteRenderer>();
        var original = prefab.GetComponent<SpriteRenderer>();
        render.sortingLayerID = original.sortingLayerID;
        render.sortingOrder = original.sortingOrder;
        render.sprite = sprite;
        // Unity builds the trigger from this sprite's silhouette at its gameplay size.
        go.AddComponent<PolygonCollider2D>().isTrigger = true;
        var shot = go.AddComponent<SkinProjectile>();
        shot.visual = render;
        shot.IsOrangeSeed = skin == "Juice";
        shot.body = go.AddComponent<Rigidbody2D>();
        shot.body.gravityScale = 0f;
        shot.body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        return shot;
    }

    Vector2 acceleration;

    public void Launch(Vector3 origin, Vector2 velocity, float gravity)
    {
        acceleration = new Vector2(0f, gravity);
        transform.SetPositionAndRotation(origin, Quaternion.FromToRotation(Vector3.up, velocity.normalized));
        gameObject.SetActive(true);
        body.position = origin;
        body.velocity = velocity;
        if (IsOrangeSeed) playCamera = Camera.main;
        expires = Time.time + 4f;
    }

    void FixedUpdate()
    {
        bool finished = IsOrangeSeed && playCamera != null ? HasLeftView() : Time.time >= expires;
        if (finished) { gameObject.SetActive(false); return; }
        body.velocity += acceleration * Time.fixedDeltaTime;
        Vector2 v = body.velocity;
        body.rotation = Mathf.Atan2(-v.x, v.y) * Mathf.Rad2Deg;
    }

    bool HasLeftView()
    {
        Bounds bounds = visual.bounds;
        Vector3 min = playCamera.WorldToViewportPoint(bounds.min);
        Vector3 max = playCamera.WorldToViewportPoint(bounds.max);
        return min.y > 1f || max.x < 0f || min.x > 1f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Ground") && body.velocity.y < 0f) gameObject.SetActive(false);
    }
}
