using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.XR;

public class Coin : MonoBehaviour
{
    private Rigidbody2D rb1;
    int value = 1;

    void Start()
    {
        float rand = Random.Range(0, 2) * 2 - 1;
        rb1 = GetComponent<Rigidbody2D>();
        rb1.AddForce(new Vector2(9.8f * 25f * rand, 9.8f * 50f));
    }

    private void OnTriggerEnter2D(Collider2D coll)
    {
        if (coll.gameObject.CompareTag("Shooter"))
        {
            Destroy(gameObject);
            Coin_Counter.coins += value;
        }
        if (coll.gameObject.CompareTag("Ground"))
        {
            gameObject.GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezePositionY;
            float CurrentY = gameObject.transform.position.y;
            transform.Translate(0, -2.9f - CurrentY, 0);
        }
    }
}
