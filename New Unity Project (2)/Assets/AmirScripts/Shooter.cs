using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    SkinAbility ability;

    void Awake()
    {
        gameObject.AddComponent<DooghSkinView>();
        ability = gameObject.AddComponent<SkinAbility>();
    }

    Vector3 bulletSpawnPoint;
    public GameObject bulletPrefab;
    // Time between two shots while the finger is held down (in seconds).
    // Based on real time, so it is the same on every phone, fast or slow.
    const float FireInterval = 0.15f;    // roughly 6.7 shots per second
    const float GasFireInterval = 0.1f;  // CO2: 10 shots per second
    float nextFizz;

    internal void ShootStandard(Vector3 position, Vector2 direction)
    {
        GetBullet(position).GetComponent<Bullet2>().dir = direction.normalized;
    }
    float lastShot = -10f;

    // Phones run games at 30 frames per second by default; ask for 60 so movement is smooth
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void SmoothFrameRate()
    {
        Application.targetFrameRate = 60;
    }

    bool pressOnUI;

    // ---- bullet pool: bullets are reused instead of created/destroyed every shot
    readonly System.Collections.Generic.List<GameObject> pool = new System.Collections.Generic.List<GameObject>();

    GameObject GetBullet(Vector3 pos)
    {
        pool.RemoveAll(b => b == null);
        foreach (GameObject b in pool)
        {
            if (!b.activeSelf)
            {
                b.transform.SetPositionAndRotation(pos, Quaternion.identity);
                b.SetActive(true);
                return b;
            }
        }
        var bullet = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bullet.AddComponent<PolygonCollider2D>();
        // A bullet must never be pushed or turned by anything (the bottle, coins, other bullets):
        // no physics pushing, no rotation, and its colliders only detect hits.
        var rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.freezeRotation = true;
        }
        foreach (var c in bullet.GetComponents<Collider2D>()) c.isTrigger = true;
        bullet.AddComponent<Bullet2>();
        pool.Add(bullet);
        return bullet;
    }

    // Bullets start at the top of the bottle (works wherever the doogh is placed)
    float MuzzleY()
    {
        var r = GetComponent<Renderer>();
        if (r == null) r = GetComponentInChildren<Renderer>();
        return r != null ? r.bounds.max.y - 0.1f : 0.3f;
    }

    void Update()
    {
        if (Time.timeScale == 0f) return;   // paused / game over
        if (Input.GetMouseButtonDown(0)) pressOnUI = PauseMenu.PointerOverUI();
        bulletSpawnPoint = new Vector3(transform.position.x, MuzzleY(), transform.position.z);
        bool gas = PowerUps.GasActive;

        // CO2: the doogh fizzes over
        if (gas && Time.time >= nextFizz)
        {
            nextFizz = Time.time + 0.04f;
            FizzBubble.At(new Vector3(transform.position.x + Random.Range(-0.15f, 0.15f), bulletSpawnPoint.y, 0f));
        }

        float interval = gas ? GasFireInterval : FireInterval;
        if (Input.GetKey(KeyCode.Mouse0) && !pressOnUI && Time.time - lastShot >= interval)
        {
            lastShot = Time.time;
            bool seed = ability.RollOrangeSeed();
            if (gas)
            {
                FirePlayerShot(new Vector2(-0.32f, 1f), seed);
                FirePlayerShot(new Vector2(0.32f, 1f), seed);
            }
            FirePlayerShot(Vector2.up, seed);
            Sfx.Play("shoot", 0.35f, 0.08f);
            //bullet.AddComponent<Rigidbody2D>();
        }
    }

    void FirePlayerShot(Vector2 direction, bool seed)
    {
        if (seed) ability.FireSeed(bulletSpawnPoint, direction);
        else ShootStandard(bulletSpawnPoint, direction);
    }
}
