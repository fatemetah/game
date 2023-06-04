using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    private Rigidbody2D rb;

    public int value;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
       
    }

    void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
            Coin_Counter.coins += 1;
        }
    }

    

}
