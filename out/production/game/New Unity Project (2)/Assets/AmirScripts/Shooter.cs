using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    Vector3 bulletSpawnPoint;
    public GameObject bulletPrefab;
    //float time = 100;
    float time = 0;
    float LastTime = 0;
    float Now = 0;
    float DeltaShoot = 15f;
    void Clock()
    {
        time++;
        Invoke("Clock", 0.01f);
    }
    void Start()
    {
        Invoke("Clock", 0);
    }
    void Update()
    {
        if (time == 100)
        {
            time -= 100;
            LastTime -= 100;
        }
        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos.z = 10;
        if (mousePos.x < -3.8f) { mousePos.x = -3.8f; }
        if (mousePos.x > 3.8f) { mousePos.x = 3.8f; }
        mousePos.y = 0.3f;
        //mousePos.x += 0.03f;
        bulletSpawnPoint = mousePos;
        Now = time;
        if (Input.GetKey(KeyCode.Mouse0) && Now-LastTime>=DeltaShoot)
        {
            LastTime = time;
            var bullet = Instantiate(bulletPrefab, bulletSpawnPoint, Quaternion.identity);
            bullet.AddComponent< Bullet2> ();
            bullet.AddComponent<PolygonCollider2D>();
            //bullet.AddComponent<Rigidbody2D>();
        }
    }
}
