using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harkat : MonoBehaviour
{
    float LastMouseX = 0f;
    float DeltaMouseX = 0f;
    void Start()
    {
        
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos = Camera.main.ScreenToWorldPoint(mousePos);
        mousePos.z += 10;
        if(mousePos.x < -3.8f){mousePos.x = -3.8f; }
        if(mousePos.x > 3.8f){mousePos.x = 3.8f;}
        DeltaMouseX = mousePos.x - LastMouseX;
        transform.Translate(DeltaMouseX,0,0);
        LastMouseX = mousePos.x;
    }
}