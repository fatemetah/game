using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;

    public GameObject pauseMenuUI;
    public GameObject loserMenuUI;
    public GameObject pauseButton;   // the ⏸ button in the top corner

    void Awake()
    {
        GameIsPaused = false;
    }

    void Update()
    {
        bool lost = loserMenuUI != null && loserMenuUI.activeSelf;

        // Escape on computer = Back button on Android
        if (Input.GetKeyDown(KeyCode.Escape) && !lost)
        {
            var picker = pauseMenuUI.GetComponentInChildren<SkinPicker>();
            if (GameIsPaused && picker != null && picker.Close()) return;
            if (!GameIsPaused) Pause();
            else if (Sub("OptionsMenu") != null && Sub("OptionsMenu").activeSelf) ShowMainCard();   // OPTIONS -> back to PAUSED
            else Resume();
        }

        if (pauseButton != null)
        {
            bool show = !GameIsPaused && !lost;
            if (pauseButton.activeSelf != show) pauseButton.SetActive(show);
        }
    }

    // When the phone goes to the home screen / a call comes in, pause the game
    void OnApplicationPause(bool paused)
    {
        if (paused && !GameIsPaused && (loserMenuUI == null || !loserMenuUI.activeSelf)) Pause();
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    // The pause window has two cards: PAUSED (MainPauseMenu) and OPTIONS (OptionsMenu)
    GameObject Sub(string name)
    {
        if (pauseMenuUI == null) return null;
        foreach (Transform t in pauseMenuUI.transform)
            if (t.name.Trim() == name) return t.gameObject;
        return null;
    }

    void ShowMainCard()
    {
        GameObject main = Sub("MainPauseMenu"), opt = Sub("OptionsMenu");
        if (opt != null) opt.SetActive(false);
        if (main != null) main.SetActive(true);
    }

    public void Pause()
    {
        ShowMainCard();   // always open on the PAUSED card, never on OPTIONS
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

    public void GoToMenu()
    {
        Coin_Counter.coins = 0;
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene(0);
    }

    public void Restart()
    {
        Coin_Counter.coins = 0;
        Time.timeScale = 1f;
        GameIsPaused = false;
        SceneManager.LoadScene(1);
    }

    // True when the finger / mouse is on a UI button (so tapping ⏸ doesn't also shoot or move)
    public static bool PointerOverUI()
    {
        var es = EventSystem.current;
        if (es == null) return false;
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
                if (es.IsPointerOverGameObject(Input.GetTouch(i).fingerId)) return true;
            return false;
        }
        return es.IsPointerOverGameObject();
    }
}
