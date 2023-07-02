using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Bullet2 : MonoBehaviour
{
    //float BulletSpeed = 5f;
    float LifeTime = 1f;
    void Awake()
    {
        Destroy(gameObject, LifeTime);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Enemy1" || collision.gameObject.tag == "Enemy2" || collision.gameObject.tag == "Enemy3")
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
    }
    
    private void FixedUpdate()
    {
        transform.Translate(0, 0.17f, 0);
    }
}