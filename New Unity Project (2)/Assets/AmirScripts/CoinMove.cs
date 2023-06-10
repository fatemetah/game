using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class CoinMove : MonoBehaviour
{
    Rigidbody2D RB;
    void Start()
    {
        RB = gameObject.GetComponent<Rigidbody2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("LeftWall")) {
            RB.AddForce(new Vector2(9.8f * 45f, 0f));
            Debug.Log(collision.gameObject.tag);
        }
        if (collision.gameObject.CompareTag("RightWall"))
        {
            RB.AddForce(new Vector2(-9.8f * 45f, 0f));
            Debug.Log(collision.gameObject.tag);
        }
    }

    void FixedUpdate()
    {
        
    }
}
