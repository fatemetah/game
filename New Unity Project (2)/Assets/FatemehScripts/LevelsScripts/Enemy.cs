﻿//using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    public float speed = 3f;
    private Rigidbody2D rb;
    public GameObject NextModel;
    public float health = 2f;
    public Transform transform;
    public ToggleScript Tgrav;
    float turni = 1;
    float activ = 1;

     void Start()
    {
        Tgrav = GetComponent<ToggleScript>();
        //turni *= -1;
        float turni = Random.Range(0, 2) * 2 - 1;
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(new Vector2(9.8f * 25f * turni, 9.8f * 50f));
        rb.gravityScale = Tgrav.Getgrav();
    }

    void activatori()
    {
        activ = 1;
    }

    void Update()
    {
        
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shooter") && (gameObject.CompareTag("Enemy2") || gameObject.CompareTag("Enemy3") || gameObject.CompareTag("Enemy1")) && activ == 1)
        {
            
            other.gameObject.SetActive(false);
            //Destroy(other.gameObject);

        }
        if (other.gameObject.CompareTag("Shooter") && gameObject.CompareTag("ghost"))
        {
            Destroy(gameObject);
            activ = 0;
            Invoke("activatori", 3);
            //other.gameObject.SetActive(false);
            //Destroy(other.gameObject);

        }
        else if (other.gameObject.CompareTag("Bullet"))
        {
            health -= 1f;
            if (health == 0f /*&& gameObject.CompareTag("Enemy1")*/)
            {
                Destroy(other.gameObject);
                Destroy(gameObject);
                if (gameObject.CompareTag("Enemy1"))
                {
                    NextLevel();
                }
                if (gameObject.CompareTag("Enemy2") || gameObject.CompareTag("Enemy3"))
                {
                    NextLevel2();
                }
            }
        }
    }

    /*private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Shooter"))
        {
            other.gameObject.SetActive(false);
            //Destroy(other.gameObject);
            
            
        } else if (other.gameObject.CompareTag("Bullet")) {
            health -= 1f;
            if (health == 0f)
            {
                Destroy(other.gameObject);
                Destroy(gameObject);
                NextLevel();
            }
        }
    }*/

     void NextLevel()
    {
        Vector3 position = transform.position;
        GameObject coin = Instantiate(NextModel, position, Quaternion.identity);
        coin.SetActive(true);
    }
    void NextLevel2()
    {

        Vector3 position = transform.position;
        GameObject coin = Instantiate(NextModel, position, Quaternion.identity);
        GameObject coin1 = Instantiate(NextModel, position, Quaternion.identity);
        coin.SetActive(true);
        coin1.SetActive(true);
    }
}




