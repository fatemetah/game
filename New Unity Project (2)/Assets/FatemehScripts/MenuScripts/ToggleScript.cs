using UnityEngine;
using UnityEngine.UI;

// Game mode screen: the ticked toggle is saved right away (no Save button needed).
// Also kept on the enemy prefabs for compatibility: Getgrav() returns the current mode's gravity.
public class ToggleScript : MonoBehaviour
{
    public float Getgrav()
    {
        return GameDifficulty.Gravity;
    }

    void OnEnable()
    {
        foreach (Toggle t in GetComponentsInChildren<Toggle>(true))
        {
            t.SetIsOnWithoutNotify(LevelOf(t) == GameDifficulty.Current);
            t.onValueChanged.RemoveListener(OnToggle);
            t.onValueChanged.AddListener(OnToggle);
        }
    }

    void OnDisable()
    {
        foreach (Toggle t in GetComponentsInChildren<Toggle>(true))
            t.onValueChanged.RemoveListener(OnToggle);
    }

    void OnToggle(bool isOn)
    {
        if (!isOn) return;
        foreach (Toggle t in GetComponentsInChildren<Toggle>(true))
        {
            if (t.isOn)
            {
                GameDifficulty.Current = LevelOf(t);
                return;
            }
        }
    }

    // Kept so old button links don't break; the mode is already saved on tick.
    public void Submit() { }

    static GameDifficulty.Level LevelOf(Toggle t)
    {
        string n = t.name.Trim();
        if (n == "EasyMode Toggle") return GameDifficulty.Level.Easy;
        if (n == "HardMode Toggle") return GameDifficulty.Level.Hard;
        if (n == "TahmaMode Toggle") return GameDifficulty.Level.Tahma;
        return GameDifficulty.Level.Medium;
    }
}
