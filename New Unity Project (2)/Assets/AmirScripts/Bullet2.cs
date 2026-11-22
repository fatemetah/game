using UnityEngine;

// A bullet flies straight up. Bullets are reused (not created and destroyed every shot),
// which avoids small hiccups on slow phones.
public class Bullet2 : MonoBehaviour
{
    const float LifeTime = 1f;
    public Vector2 dir = Vector2.up;   // straight up, or tilted for the CO2 side shots
    float bornAt;

    void OnEnable()
    {
        bornAt = Time.time;
        dir = Vector2.up;
    }

    // Put a bullet back in the pool (or destroy it if it isn't a pooled bullet)
    public static void Recycle(GameObject bullet)
    {
        if (bullet == null) return;
        if (bullet.GetComponent<Bullet2>() != null || bullet.GetComponent<SkinProjectile>() != null) bullet.SetActive(false);
        else Object.Destroy(bullet);
    }

    // (A hit is handled by the enemy: it counts the bullet once and turns it off.)

    void FixedUpdate()
    {
        if (Time.time - bornAt >= LifeTime) { gameObject.SetActive(false); return; }
        transform.rotation = Quaternion.FromToRotation(Vector3.up, dir);   // points where it flies
        transform.Translate(dir * 0.17f, Space.World);
    }
}
