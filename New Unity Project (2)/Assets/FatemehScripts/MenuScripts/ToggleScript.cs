using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI; 

public class ToggleScript : MonoBehaviour
{
    private float grav = 0.5f;
    public float Getgrav()
    {
        return grav;
    }
    void SetGrav(float gra)
    {
        grav = gra;
    }
    ToggleGroup toggleGroup;

     void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();
    }

    public void Submit()
    {
        Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
        if (toggle.name == "HardMode Toggle") { Debug.Log(toggle.name); SetGrav(1.2f); Debug.Log(grav);}
        if (toggle.name == "MediumMode Toggle") { SetGrav(0.5f); Debug.Log(grav); }
        if (toggle.name == "EasyMode Toggle") { SetGrav(0.3f); Debug.Log(grav); }
        if (toggle.name == "TahmaMode Toggle") { SetGrav(2f); Debug.Log(grav); }
        Debug.Log(toggle.name + "_" + toggle.GetComponentInChildren<Text>().text);
    }


}
