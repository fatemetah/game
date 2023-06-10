using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelRotation : MonoBehaviour
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
        if(mousePos.x < -2.45f) {mousePos.x = -2.45f; }
        if(mousePos.x > 2.45f) {mousePos.x = 2.45f;}
        DeltaMouseX = mousePos.x - LastMouseX;
        transform.Rotate(0,0,-(DeltaMouseX/2.927878f)*360);
        LastMouseX = mousePos.x;
    }
}