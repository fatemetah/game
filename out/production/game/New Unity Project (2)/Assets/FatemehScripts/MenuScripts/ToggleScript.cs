using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.UI; 

public class ToggleScript : MonoBehaviour
{

    ToggleGroup toggleGroup;

     void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();
    }

    public void Submit()
    {
        Toggle toggle = toggleGroup.ActiveToggles().FirstOrDefault();
        Debug.Log(toggle.name + "_" + toggle.GetComponentInChildren<Text>().text);
    }


}
