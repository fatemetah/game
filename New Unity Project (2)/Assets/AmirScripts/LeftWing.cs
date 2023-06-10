using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LeftWing : MonoBehaviour
{
    public GameObject leftWing;
    float Rot = 2f;
    void Start()
    {
        
    }
    void FixedUpdate()
    {
        transform.Rotate(0, 0, Rot);
        if(leftWing.transform.rotation.z<-0.07)
        {
            Rot *=-1;
        }
        if (leftWing.transform.rotation.z > 0.3)
        {
            Rot *= -1;
        }
    }
}