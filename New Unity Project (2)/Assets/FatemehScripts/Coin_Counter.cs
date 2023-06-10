using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Coin_Counter : MonoBehaviour
{

    Text text;
    public static int coins;

    void Start()
    {
        text = GetComponent<Text>();
    }

    void Update()
    {
        text.text = coins.ToString();
    }
}
