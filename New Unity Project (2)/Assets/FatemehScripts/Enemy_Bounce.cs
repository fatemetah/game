using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System;

public class Enemy_Bounce : MonoBehaviour
{
    Boolean booli = true;
    private Rigidbody2D rb;
    Vector3 lastVelocity;
    BoxCollider2D BC;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }


    void Update()
    {
        lastVelocity = rb.velocity;
    }
    void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("LeftWall"))
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }
        if (coll.gameObject.CompareTag("RightWall"))
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }
        if (coll.gameObject.CompareTag("Ground"))
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
        }
    }
    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.CompareTag("LeftWall"))
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            var speed = lastVelocity.magnitude;
            var direction = Vector3.Reflect(lastVelocity.normalized, coll.contacts[0].normal);

            rb.velocity = direction * Mathf.Max(speed, 0f);
        }
        if (coll.gameObject.CompareTag("RightWall"))
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            var speed = lastVelocity.magnitude;
            var direction = Vector3.Reflect(lastVelocity.normalized, coll.contacts[0].normal);

            rb.velocity = direction * Mathf.Max(speed, 0f);
        }
        if (coll.gameObject.CompareTag("Ground"))
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            var speed = lastVelocity.magnitude*8;
            if (booli)
            {
                speed -= lastVelocity.magnitude;
                booli = false;
            }
            var direction = Vector3.Reflect(lastVelocity.normalized, coll.contacts[0].normal);

            rb.velocity = direction * Mathf.Max(speed*0.125f, 0f);
        }
    }
}