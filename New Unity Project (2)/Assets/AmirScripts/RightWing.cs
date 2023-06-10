using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RightWing : MonoBehaviour
{
    public GameObject rightWing;
    float Rot = -2f;
    void Start()
    {

    }
    void FixedUpdate()
    {
        transform.Rotate(0, 0, Rot);
        //Debug.Log(rightWing.transform.rotation.z);
        if (rightWing.transform.rotation.z > 0.07)
        {
            Rot *= -1;
        }
        if (rightWing.transform.rotation.z < -0.3)
        {
            Rot *= -1;
        }
    }
}
