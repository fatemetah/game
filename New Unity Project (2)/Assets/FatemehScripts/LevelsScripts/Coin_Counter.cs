using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The score at the top of the screen. Only redraws when the number changes,
// and gives a little "pop" every time you catch a coin.
public class Coin_Counter : MonoBehaviour
{
    public static int coins;
    const string WalletKey = "CoinWalletBalance";
    public static int WalletBalance
    {
        get => PlayerPrefs.GetInt(WalletKey, 0);
        internal set => PlayerPrefs.SetInt(WalletKey, value);
    }
    public static int WalletEarned { get; private set; }

    void Awake()
    {
        coins = 0;
        WalletEarned = 0;
    }

    public static void Collect(int count)
    {
        int multiplier = 1 + (int)GameDifficulty.Current;
        int credit = count * multiplier;
        coins += count;
        WalletEarned += credit;
        WalletBalance = (int)System.Math.Min(int.MaxValue, (long)WalletBalance + credit);
        PlayerPrefs.Save();
    }

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
