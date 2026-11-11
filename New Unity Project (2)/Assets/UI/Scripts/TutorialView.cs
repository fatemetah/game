using TMPro;
using UnityEngine;

// "How to play" cards shown only the very first time the game is opened.
public class TutorialView : MonoBehaviour
{
    public const string PrefKey = "TutorialSeen";

    public GameObject[] pages;
    public TMP_Text nextLabel;
    public TMP_Text counter;

    int index;
    bool shown;

    void Awake()
    {
        if (PlayerPrefs.GetInt(PrefKey, 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }
        shown = true;
        transform.SetAsLastSibling();
        ShowPage(0);
    }

    public void Next()
    {
        if (index >= pages.Length - 1) { Finish(); return; }
        ShowPage(index + 1);
    }

    public void Skip() { Finish(); }

    void Finish()
    {
        PlayerPrefs.SetInt(PrefKey, 1);
        PlayerPrefs.Save();
        gameObject.SetActive(false);
    }

    // Closed some other way (e.g. the phone's back key): still counts as seen
    void OnDisable()
    {
        if (shown && PlayerPrefs.GetInt(PrefKey, 0) == 0)
        {
            PlayerPrefs.SetInt(PrefKey, 1);
            PlayerPrefs.Save();
        }
    }

    void ShowPage(int n)
    {
        index = n;
        for (int i = 0; i < pages.Length; i++) pages[i].SetActive(i == n);
        bool last = n == pages.Length - 1;
        if (nextLabel != null) nextLabel.text = last ? "LET'S GO!" : "NEXT";
        if (counter != null) counter.text = (n + 1) + " / " + pages.Length;
    }
}
