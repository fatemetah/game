using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    const string MusicKey = "MusicVolume";
    const string EffectsKey = "EffectsVolume";
    Slider musicSlider, effectsSlider;
    public static event System.Action<float> MusicVolumeChanged;
    public static float MusicGain => ToLinear(Read(MusicKey));
    public static float EffectsGain => ToLinear(Read(EffectsKey));

    static float Read(string key) => PlayerPrefs.GetFloat(key, PlayerPrefs.GetFloat("Volume", 0f));

    void Awake()
    {
        bool main = gameObject.scene.name == "Menu";
        effectsSlider = transform.Find("Volume Slider").GetComponent<Slider>();
        var effectsLabel = transform.Find("Volume Text").GetComponent<TMP_Text>();
        musicSlider = Instantiate(effectsSlider, transform);
        musicSlider.name = "Music Slider";
        musicSlider.onValueChanged = new Slider.SliderEvent();
        musicSlider.onValueChanged.AddListener(SetMusic);
        var musicLabel = Instantiate(effectsLabel, transform);
        musicLabel.name = "Music Label";
        musicLabel.text = "MUSIC";
        effectsLabel.text = "SOUND EFFECTS";
        musicLabel.fontSize = effectsLabel.fontSize = 55f;
        Position(musicLabel.rectTransform, main ? 400f : 205f);
        Position((RectTransform)musicSlider.transform, main ? 323f : 125f);
        Position(effectsLabel.rectTransform, main ? 230f : 0f);
        Position((RectTransform)effectsSlider.transform, main ? 153f : -80f);
    }

    static void Position(RectTransform rect, float y)
    {
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
    }

    void Start()
    {
        if (gameObject.scene.name != "Menu") return;
        var template = transform.Find("ChangMode Button") ?? transform.Find("Back Button");
        if (template == null) return;
        var sample = template.GetComponent<Button>();
        var font = sample.GetComponentInChildren<TMP_Text>(true).font;
        var picker = gameObject.AddComponent<SkinPicker>();
        var backgrounds = gameObject.AddComponent<SkinPicker>();
        Position((RectTransform)template, -113f);
        Position((RectTransform)transform.Find("Back Button"), -651f);
        AddButton(sample, "Background Button", "BACKGROUND", -381f, () => backgrounds.Open(font, true));
        AddButton(sample, "Skin Button", "SKIN", -247f, () => picker.Open(font));
        var tutorial = transform.parent.GetComponentInChildren<TutorialView>(true);
        if (tutorial != null)
            AddButton(sample, "Replay Tour Button", "REPLAY TOUR", -515f, () => tutorial.Replay(gameObject));
    }

    void AddButton(Button sample, string buttonName, string caption, float y, UnityEngine.Events.UnityAction action)
    {
        var button = Instantiate(sample, transform);
        button.name = buttonName;
        button.onClick = new Button.ButtonClickedEvent();
        button.onClick.AddListener(action);
        var rect = (RectTransform)button.transform;
        rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, y);
        button.GetComponentInChildren<TMP_Text>(true).text = caption;
    }

    void OnEnable()
    {
        musicSlider.SetValueWithoutNotify(Read(MusicKey));
        effectsSlider.SetValueWithoutNotify(Read(EffectsKey));
    }

    // Both scenes already bind the original slider to this callback.
    public void SetVolume(float volume)
    {
        PlayerPrefs.SetFloat(EffectsKey, volume);
        PlayerPrefs.Save();
        Sfx.SetGain(ToLinear(volume));
    }

    void SetMusic(float volume)
    {
        PlayerPrefs.SetFloat(MusicKey, volume);
        PlayerPrefs.Save();
        MusicVolumeChanged?.Invoke(ToLinear(volume));
    }

    // slider -80..0 -> 0..1, with a natural-feeling curve
    static float ToLinear(float sliderValue)
    {
        float t = Mathf.InverseLerp(-80f, 0f, sliderValue);
        return t * t;
    }
}
