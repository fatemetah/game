using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The score at the top of the screen. Only redraws when the number changes,
// and gives a little "pop" every time you catch a coin.
public class Coin_Counter : MonoBehaviour
{
    public static int coins;

    public TMP_Text label;      // comic number (new score badge)
    Text legacy;                // old plain text, if this is still the old counter
    int shown = -1;
    float pop;

    void Start()
    {
        if (label == null) label = GetComponentInChildren<TMP_Text>(true);
        legacy = GetComponent<Text>();
    }

    void Update()
    {
        if (coins != shown)
        {
            if (shown >= 0 && coins > shown) pop = 1f;
            shown = coins;
            string s = coins.ToString();
            if (label != null) label.text = s;
            if (legacy != null) legacy.text = s;
        }
        if (pop > 0f)
        {
            pop = Mathf.Max(0f, pop - Time.unscaledDeltaTime * 6f);
            transform.localScale = Vector3.one * (1f + 0.25f * pop);
        }
    }
}
