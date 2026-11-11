using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinMove : MonoBehaviour
{
    Rigidbody2D RB;
    void Start()
    {
        // Coins disappear 3 seconds after landing if not picked up
        if (GetComponent<TimedDespawn>() == null) gameObject.AddComponent<TimedDespawn>().lifetime = 3f;
        RB = gameObject.GetComponent<Rigidbody2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("LeftWall")) {
            RB.AddForce(new Vector2(9.8f * 45f, 0f));
        }
        if (collision.gameObject.CompareTag("RightWall"))
        {
            RB.AddForce(new Vector2(-9.8f * 45f, 0f));
        }
    }

}
