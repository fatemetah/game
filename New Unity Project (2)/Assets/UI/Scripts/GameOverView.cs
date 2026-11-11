using TMPro;
using UnityEngine;

// The comic "GAME OVER" card shown when the player loses.
public class GameOverView : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text scoreText;
    public TMP_Text bestText;
    public GameObject recordBadge;
    public GameObject scoresPanel;

    static readonly string[] modeNames = { "EASY", "MEDIUM", "HARD", "TAHMA!!" };

    public void Show(string player, int score, bool record, GameDifficulty.Level level)
    {
        nameText.text = PersianText.Fix(player);
        scoreText.text = score.ToString();
        recordBadge.SetActive(record);

        var top = ScoreBoard.Top(level, 1);
        bestText.text = top.Count > 0
            ? "BEST (" + modeNames[(int)level] + "):  " + PersianText.Fix(top[0].name) + "  " + top[0].score
            : "";

        if (scoresPanel != null) scoresPanel.SetActive(false);
    }
}
