using UnityEngine;
using UnityEngine.UI;

// The VIBRATION tick box in OPTIONS.
[RequireComponent(typeof(Toggle))]
public class VibrationToggle : MonoBehaviour
{
    Toggle toggle;

    void Awake()
    {
        toggle = GetComponent<Toggle>();
        toggle.onValueChanged.AddListener(on =>
        {
            Haptics.Enabled = on;
            if (on) Haptics.Buzz();   // a short buzz so you feel that it is on
        });
    }

    void OnEnable()
    {
        if (toggle == null) toggle = GetComponent<Toggle>();
        toggle.SetIsOnWithoutNotify(Haptics.Enabled);
    }
}
