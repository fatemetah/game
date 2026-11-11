using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LoserMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject Player;
    public GameOverView view;   // the comic GAME OVER card

    private bool shown = false;

    void Update()
    {
        if (!shown && Player.activeSelf == false)
        {
            Pause();
        }
        // Phone back key on the GAME OVER screen goes back to the menu
        else if (shown && Input.GetKeyDown(KeyCode.Escape))
        {
            GoToMenu();
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Player.SetActive(true);
        Coin_Counter.coins = 0;
        Time.timeScale = 1f;
        GameIsPaused = false;
        shown = false;
    }

    void Pause()
    {
        shown = true;
        Sfx.Play("lose", 0.9f, 0f);
        Haptics.Buzz();   // vibrate (can be turned off in OPTIONS)
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
        ShowFinalScore();
    }

    // Writes the score under "YOUR SCORE", saves it to the SCORES table
    // and hides the in-game counter at the top.
    void ShowFinalScore()
    {
        int score = Coin_Counter.coins;
        string player = NamePopup.CurrentName;
        bool record = ScoreBoard.Submit(player, GameDifficulty.Current, score);

        if (view != null)
        {
            view.Show(player, score, record, GameDifficulty.Current);
        }
        else
        {
            Transform t = pauseMenuUI.transform.Find("MainLoserMenu/ShowScore Text");
            TMP_Text text = t != null ? t.GetComponent<TMP_Text>() : null;
            if (text != null)
            {
                text.enableWordWrapping = false;
                text.text = "YOUR SCORE\n<size=150%>" + score + "</size>"
                    + (record ? "\n<size=70%><color=#FFD600>NEW RECORD!</color></size>" : "");
            }
        }

        Coin_Counter counter = FindObjectOfType<Coin_Counter>();
        if (counter != null) counter.gameObject.SetActive(false);
    }

    // Lets the next person type their name: back to the menu with the name window open.
    public void ChangeName()
    {
        Resume();
        NamePopup.openOnLoad = true;
        SceneManager.LoadScene(0);
    }

    public void GoToMenu()
    {
        Resume();
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        Resume();
        SceneManager.LoadScene(1);
    }
}
