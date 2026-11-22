using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

// "YOUR NAME" window shown before every game.
// Accepts Persian letters, English letters (always capital), numbers and spaces.
// PC: type with the keyboard. Phone: tap the box to open the phone keyboard.
public class NamePopup : MonoBehaviour
{
    public const string PrefKey = "PlayerName";
    public const int MaxLength = 10;

    // Set by the lose screen's CHANGE NAME button so the window opens when the menu loads.
    public static bool openOnLoad = false;

    public RectTransform card;
    public TMP_Text display;      // shows the name (Persian joined, right-to-left)
    public GameObject placeholder;

    string value = "";
    TouchScreenKeyboard keyboard;

    void Awake()
    {
        var go = (RectTransform)card.Find("Go Button");
        var back = (RectTransform)transform.Find("Back Button");
        back.SetParent(card, false);
        back.sizeDelta = new Vector2(235f, 94f);
        back.anchoredPosition = new Vector2(-206f, -169f);
        go.sizeDelta = new Vector2(360f, 127f);
        go.anchoredPosition = new Vector2(122f, -169f);
    }

    public static string CurrentName
    {
        get { return PlayerPrefs.GetString(PrefKey, "PLAYER"); }
    }

    public void Show()
    {
        gameObject.SetActive(true);
        transform.SetAsLastSibling();
        value = Clean(PlayerPrefs.GetString(PrefKey, ""));
        Refresh();
        OpenKeyboard();
    }

    public void Hide()
    {
        CloseKeyboard();
        gameObject.SetActive(false);
    }

    // Called when the name box is tapped
    public void OpenKeyboard()
    {
        if (!TouchScreenKeyboard.isSupported) return;
        if (keyboard != null && keyboard.active) return;
        keyboard = TouchScreenKeyboard.Open(value, TouchScreenKeyboardType.Default, false, false, false, false, "", MaxLength);
    }

    void CloseKeyboard()
    {
        if (keyboard != null && keyboard.active) keyboard.active = false;
        keyboard = null;
    }

    void Update()
    {
        if (keyboard != null)
        {
            // phone keyboard
            string typed = Clean(keyboard.text);
            if (typed != value) { value = typed; Refresh(); }
            if (keyboard.status == TouchScreenKeyboard.Status.Done) { keyboard = null; Confirm(); }
            else if (keyboard.status != TouchScreenKeyboard.Status.Visible) keyboard = null;
            return;
        }

        // computer keyboard
        string input = Input.inputString;
        if (string.IsNullOrEmpty(input)) return;
        bool changed = false;
        foreach (char ch in input)
        {
            if (ch == '\b')
            {
                if (value.Length > 0) { value = value.Substring(0, value.Length - 1); changed = true; }
            }
            else if (ch == '\n' || ch == '\r')
            {
                Confirm();
                return;
            }
            else
            {
                char c = Allowed(ch);
                if (c != '\0' && value.Length < MaxLength) { value += c; changed = true; }
            }
        }
        if (changed) Refresh();
    }

    void Refresh()
    {
        display.text = PersianText.Fix(value);
        if (placeholder != null) placeholder.SetActive(value.Length == 0);
    }

    public void Confirm()
    {
        string name = Clean(value).Trim();
        if (name.Length == 0)
        {
            StopAllCoroutines();
            StartCoroutine(Shake());
            return;
        }
        CloseKeyboard();
        PlayerPrefs.SetString(PrefKey, name);
        PlayerPrefs.Save();
        SceneManager.LoadScene(1);
    }

    static char Allowed(char c)
    {
        c = PersianText.Normalize(char.ToUpperInvariant(c));
        if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || c == ' ') return c;
        if (PersianText.IsPersianLetter(c)) return c;
        if (c >= '۰' && c <= '۹') return c;   // Persian digits
        if (c == '‌') return c;                      // half-space (نیم‌فاصله)
        return '\0';
    }

    public static string Clean(string s)
    {
        var sb = new System.Text.StringBuilder();
        foreach (char ch in s ?? "")
        {
            char c = Allowed(ch);
            if (c != '\0' && sb.Length < MaxLength) sb.Append(c);
        }
        return sb.ToString();
    }

    IEnumerator Shake()
    {
        Vector2 start = card.anchoredPosition;
        float t = 0f;
        while (t < 0.35f)
        {
            t += Time.unscaledDeltaTime;
            card.anchoredPosition = start + new Vector2(Mathf.Sin(t * 70f) * 18f * (1f - t / 0.35f), 0f);
            yield return null;
        }
        card.anchoredPosition = start;
    }
}
