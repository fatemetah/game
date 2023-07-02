using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;



public class LoserMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject Player;



    void Update()
    {



        if (Player.activeSelf == false)
        {
            Pause();
        }

    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Player.SetActive(true);
        Coin_Counter.coins = 0;
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
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
