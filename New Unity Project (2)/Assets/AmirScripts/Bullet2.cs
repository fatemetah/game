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
    void OnCollisionEnter2D(Collision2D collision)
    {
        //Debug.Log("khord");
        if (collision.gameObject.tag == "Enemy") {
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