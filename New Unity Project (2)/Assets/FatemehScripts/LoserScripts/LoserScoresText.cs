using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Lose screen > SCORES: lists the top 10 of the mode that was just played.
public class LoserScoresText : MonoBehaviour
{
    void OnEnable()
    {
        GameDifficulty.Level level = GameDifficulty.Current;
        var top = ScoreBoard.Top(level, 10);

        string s = level.ToString().ToUpper() + " TOP 10\n";
        if (top.Count == 0) s += "\nNO SCORES YET";
        for (int i = 0; i < top.Count; i++)
            s += "\n" + (i + 1) + ". " + PersianText.Fix(top[i].name) + "   " + top[i].score;

        // Give the list enough room (the original text box was very small)
        RectTransform rt = (RectTransform)transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, 250f);
        rt.sizeDelta = new Vector2(560f, 420f);

        TMP_Text tmp = GetComponent<TMP_Text>();
        if (tmp != null)
        {
            tmp.enableWordWrapping = false;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 10;
            tmp.fontSizeMax = 42;
            tmp.alignment = TextAlignmentOptions.Top;
            tmp.text = s;
            return;
        }
        Text legacy = GetComponent<Text>();
        if (legacy != null)
        {
            legacy.resizeTextForBestFit = true;
            legacy.text = s;
        }
    }
}
