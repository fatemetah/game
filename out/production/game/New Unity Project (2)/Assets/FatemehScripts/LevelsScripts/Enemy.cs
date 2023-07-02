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


     void Start()
    {
        float rand = Random.Range(0, 2) * 2 - 1;
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(new Vector2(9.8f * 25f * rand, 9.8f * 50f));
    }

     void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Shooter"))
        {
            
            other.gameObject.SetActive(false);
            //Destroy(other.gameObject);

        }
        else if (other.gameObject.CompareTag("Bullet"))
        {
            health -= 1f;
            if (health == 0f /*&& gameObject.CompareTag("Enemy1")*/)
            {
                Destroy(other.gameObject);
                Destroy(gameObject);
                NextLevel();
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
}




