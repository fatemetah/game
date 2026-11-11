using UnityEngine;

// Phone vibration, with an on/off setting saved on the phone (OPTIONS > VIBRATION).
public static class Haptics
{
    const string PrefKey = "Vibration";

    public static bool Enabled
    {
        get { return PlayerPrefs.GetInt(PrefKey, 1) == 1; }
        set { PlayerPrefs.SetInt(PrefKey, value ? 1 : 0); PlayerPrefs.Save(); }
    }

    public static void Buzz()
    {
        if (!Enabled) return;
#if UNITY_ANDROID || UNITY_IOS
        Handheld.Vibrate();
#endif
    }
}
