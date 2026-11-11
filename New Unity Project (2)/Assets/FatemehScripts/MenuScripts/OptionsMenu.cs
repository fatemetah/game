using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;

    const string PrefKey = "Volume";   // slider value, -80 (silent) .. 0 (full)

    void OnEnable()
    {
        // Put the slider where the player left it
        Slider slider = GetComponentInChildren<Slider>(true);
        if (slider != null) slider.SetValueWithoutNotify(PlayerPrefs.GetFloat(PrefKey, 0f));
    }

    public void SetVolume(float volume)
    {
        if (audioMixer != null) audioMixer.SetFloat("volume", volume);
        PlayerPrefs.SetFloat(PrefKey, volume);
        PlayerPrefs.Save();
        AudioListener.volume = ToLinear(volume);
    }

    public static float SavedVolume01()
    {
        return ToLinear(PlayerPrefs.GetFloat(PrefKey, 0f));
    }

    // slider -80..0 -> 0..1, with a natural-feeling curve
    static float ToLinear(float sliderValue)
    {
        float t = Mathf.InverseLerp(-80f, 0f, sliderValue);
        return t * t;
    }
}
