using TMPro;
using UnityEngine;
using UnityEngine.UI;

// SCORES screen: tabs for each game mode, and the best player of each mode on top.
public class LeaderboardView : MonoBehaviour
{
    public Image[] tabImages;
    public TMP_Text[] tabTexts;
    public GameObject[] rows;
    public Image[] rowImages;
    public TMP_Text[] rowNames;
    public TMP_Text[] rowScores;
    public TMP_Text emptyText;

    static readonly Color Yellow = new Color32(255, 214, 0, 255);
    static readonly Color Ink = new Color32(20, 20, 30, 255);

    void OnEnable()
    {
        Show((int)GameDifficulty.Current);
    }

    public void Show(int level)
    {
        for (int i = 0; i < tabImages.Length; i++)
        {
            bool on = i == level;
            tabImages[i].color = on ? Yellow : Color.white;
            tabTexts[i].color = Ink;
            tabImages[i].transform.localScale = on ? Vector3.one * 1.06f : Vector3.one;
        }

        var top = ScoreBoard.Top((GameDifficulty.Level)level, rows.Length);
        for (int i = 0; i < rows.Length; i++)
        {
            bool has = i < top.Count;
            rows[i].SetActive(has);
            if (!has) continue;
            rowNames[i].text = (i + 1) + ".  " + PersianText.Fix(top[i].name);
            rowScores[i].text = top[i].score.ToString();
            rowImages[i].color = i == 0 ? Yellow : Color.white;
        }
        emptyText.gameObject.SetActive(top.Count == 0);
    }
}
