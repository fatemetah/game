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

    TMP_Text walletText;

    static readonly string[] modeNames = { "EASY", "MEDIUM", "HARD", "TAHMA!!" };

    public void Show(string player, int score, bool record, GameDifficulty.Level level)
    {
        nameText.text = PersianText.Fix(player);
        scoreText.enableAutoSizing = true;
        scoreText.fontSizeMin = 48f;
        scoreText.fontSizeMax = 160f;
        scoreText.enableWordWrapping = false;
        scoreText.text = score + "  <size=35%><color=#336B48>+" + Coin_Counter.WalletEarned + " WALLET</color></size>";
        recordBadge.SetActive(record);

        var top = ScoreBoard.Top(level, 1);
        bestText.text = top.Count > 0
            ? "BEST (" + modeNames[(int)level] + "):  " + PersianText.Fix(top[0].name) + "  " + top[0].score
            : "";

        if (walletText == null)
        {
            walletText = Instantiate(bestText, bestText.transform.parent);
            walletText.name = "Wallet Summary";
            var footer = walletText.rectTransform;
            footer.anchorMin = footer.anchorMax = footer.pivot = new Vector2(0.5f, 0f);
            footer.sizeDelta = new Vector2(860f, 66f);
            footer.anchoredPosition = new Vector2(0f, 16f);
            walletText.color = Color.white;
            if (scoresPanel != null && scoresPanel.transform.parent == footer.parent)
                footer.SetSiblingIndex(scoresPanel.transform.GetSiblingIndex());
            walletText.fontSize = 54f;
            walletText.raycastTarget = false;
        }
        walletText.text = "WALLET: " + Coin_Counter.WalletBalance;

        if (scoresPanel != null) scoresPanel.SetActive(false);
    }
}
