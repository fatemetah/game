using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DooghYMove : MonoBehaviour
{
    public GameObject leftWing;
    float MoveY = 0.008f;
    void Start()
    {

    }
    void FixedUpdate()
    {
        transform.Translate(0,MoveY,0);
        if (leftWing.transform.rotation.z < -0.07)
        {
            MoveY *= -1f;
        }
        if (leftWing.transform.rotation.z > 0.3)
        {
            MoveY *= -1f;
        }
    }
}
