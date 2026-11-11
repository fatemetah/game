using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// Plays a "click" for every button and tick box in every scene.
// One invisible helper watches the EventSystem: whenever a button or tick box is pressed,
// it clicks. Works for buttons made later too, and nothing can unhook it.
public static class UIClickSound
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Init()
    {
        if (Object.FindObjectOfType<ClickSoundRunner>() != null) return;
        var go = new GameObject("UI Click Sound");
        Object.DontDestroyOnLoad(go);
        go.AddComponent<ClickSoundRunner>();
    }

    public static void Play() { Sfx.Play("click", 0.6f, 0.05f); }
}

public class ClickSoundRunner : MonoBehaviour
{
    // Unity selects a button the moment it is pressed, so we read the selection
    // right after the EventSystem has handled the press (LateUpdate runs after it).
    Selectable pressed;

    void LateUpdate()
    {
        var es = EventSystem.current;
        if (Input.GetMouseButtonDown(0))
        {
            pressed = null;
            GameObject sel = es != null ? es.currentSelectedGameObject : null;
            if (sel != null)
            {
                var s = sel.GetComponent<Selectable>();
                if ((s is Button || s is Toggle) && s.IsInteractable()) pressed = s;
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            if (pressed != null) UIClickSound.Play();
            pressed = null;
        }
    }
}

// kept so older scenes/scripts that mention it still compile
public class ClickSoundTag : MonoBehaviour { }
