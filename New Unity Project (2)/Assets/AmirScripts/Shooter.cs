using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    Vector3 bulletSpawnPoint;
    public GameObject bulletPrefab;
    // Time between two shots while the finger is held down (in seconds).
    // Based on real time, so it is the same on every phone, fast or slow.
    const float FireInterval = 0.2f;     // 5 shots per second
    const float GasFireInterval = 0.1f;  // CO2: 10 shots per second
    float nextFizz;

    static void SetDir(GameObject bullet, Vector2 d)
    {
        var b = bullet.GetComponent<Bullet2>();
        if (b != null) b.dir = d.normalized;
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
        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos.z = 10;
        if (mousePos.x < -3.8f) { mousePos.x = -3.8f; }
        if (mousePos.x > 3.8f) { mousePos.x = 3.8f; }
        mousePos.y = MuzzleY();   // top of the bottle
        //mousePos.x += 0.03f;
        mousePos.x = transform.position.x;   // bullets always come out of the doogh
        bulletSpawnPoint = mousePos;
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
            if (gas)
            {
                // three directions: straight up and a little to each side
                SetDir(GetBullet(bulletSpawnPoint), new Vector2(-0.32f, 1f));
                SetDir(GetBullet(bulletSpawnPoint), Vector2.up);
                SetDir(GetBullet(bulletSpawnPoint), new Vector2(0.32f, 1f));
            }
            else GetBullet(bulletSpawnPoint);
            Sfx.Play("shoot", 0.35f, 0.08f);
            //bullet.AddComponent<Rigidbody2D>();
        }
    }
}
