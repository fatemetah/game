using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The tour opens automatically on the first visit and can be replayed from Options.
public class TutorialView : MonoBehaviour
{
    public const string PrefKey = "TutorialSeen";

    public GameObject[] pages;
    public TMP_Text nextLabel;
    public TMP_Text counter;

    int index;
    bool firstVisit;
    GameObject returnMenu;

    void Awake()
    {
        if (PlayerPrefs.GetInt(PrefKey, 0) == 1)
        {
            gameObject.SetActive(false);
            return;
        }
        firstVisit = true;
        transform.SetAsLastSibling();
        ShowPage(0);
    }

    public void Replay(GameObject options)
    {
        returnMenu = options;
        options.SetActive(false);
        gameObject.SetActive(true);
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
        gameObject.SetActive(false);
        if (returnMenu != null)
        {
            returnMenu.SetActive(true);
            returnMenu = null;
        }
    }

    // Closed some other way (e.g. the phone's back key): still counts as seen
    void OnDisable()
    {
        if (firstVisit)
        {
            firstVisit = false;
            PlayerPrefs.SetInt(PrefKey, 1);
            PlayerPrefs.Save();
        }
    }

    void ShowPage(int n)
    {
        if (n == 0) AddWalletAndSkins();
        index = n;
        for (int i = 0; i < pages.Length; i++) pages[i].SetActive(i == n);
        bool last = n == pages.Length - 1;
        if (nextLabel != null) nextLabel.text = last ? (returnMenu != null ? "DONE" : "LET'S GO!") : "NEXT";
        if (counter != null) counter.text = (n + 1) + " / " + pages.Length;
    }

    void AddWalletAndSkins()
    {
        if (System.Array.Exists(pages, page => page.name == "Page Skins")) return;
        Transform welcome = pages[0].transform;
        foreach (var page in pages)
        {
            foreach (var label in page.GetComponentsInChildren<TMP_Text>(true))
            {
                if (label.enableWordWrapping && label.name != "Title" && label.transform.parent == page.transform)
                    FitParagraph(label);
            }
        }
        var scoring = welcome.parent.Find("Page Score/Text").GetComponent<TMP_Text>();
        scoring.text = "Each hit dents a can. Red cans take four hits. Catch the coins that drop for points.\n\nCoins disappear three seconds after landing.\n\nIf a can or sugar cube touches you, the game ends.";
        var titleStyle = welcome.Find("Title").GetComponent<TMP_Text>();
        var bodyStyle = welcome.Find("Text").GetComponent<TMP_Text>();
        var wallet = NewPage(titleStyle, "Wallet");
        var coin = welcome.parent.Find("Page Score/Coin").GetComponent<Image>().sprite;
        Picture(wallet, coin, 0, 317, 125);
        Paragraph(wallet, bodyStyle, "Move the doogh into coins to collect them. Each coin scores one point.", 166, 160, 45);
        Paragraph(wallet, bodyStyle, "Your wallet saves each pickup at these rates:\nEasy  x1       Medium  x2\nHard  x3       Tahma!!  x4", -57, 235, 48);
        Paragraph(wallet, bodyStyle, "12 coins on Hard add 36 coins to your wallet.", -279, 135, 49);

        var skins = NewPage(titleStyle, "Skins");
        Picture(skins, DooghSkins.Art("Mint"), -202, 130, 184);
        Picture(skins, DooghSkins.Art("Ayran"), 0, 130, 198);
        Picture(skins, DooghSkins.Art("Milk"), 202, 130, 184);
        Paragraph(skins, bodyStyle, "There are different skins. Each skin has its own special ability.", -137, 280, 61);

        int count = pages.Length;
        System.Array.Resize(ref pages, count + 2);
        pages[count] = wallet.gameObject;
        pages[count + 1] = skins.gameObject;
    }

    RectTransform NewPage(TMP_Text titleStyle, string title)
    {
        var page = (RectTransform)new GameObject("Page " + title, typeof(RectTransform)).transform;
        page.gameObject.layer = titleStyle.gameObject.layer;
        page.SetParent(pages[0].transform.parent, false);
        page.anchorMin = Vector2.zero;
        page.anchorMax = Vector2.one;
        page.offsetMin = page.offsetMax = Vector2.zero;
        Instantiate(titleStyle, page).text = title.ToUpperInvariant();
        return page;
    }

    static void Paragraph(RectTransform page, TMP_Text style, string text, float y, float height, float size)
    {
        var label = Instantiate(style, page);
        label.rectTransform.anchoredPosition = new Vector2(0, y);
        label.rectTransform.sizeDelta = new Vector2(690, height);
        label.fontSize = size;
        FitParagraph(label);
        label.text = text;
    }

    static void FitParagraph(TMP_Text label)
    {
        label.fontSizeMax = label.fontSize;
        label.fontSizeMin = label.fontSize * 0.72f;
        label.enableAutoSizing = true;
    }

    static void Picture(RectTransform page, Sprite sprite, float x, float y, float size)
    {
        var image = new GameObject("Preview", typeof(RectTransform), typeof(Image)).GetComponent<Image>();
        image.gameObject.layer = page.gameObject.layer;
        image.rectTransform.SetParent(page, false);
        image.rectTransform.anchoredPosition = new Vector2(x, y);
        image.rectTransform.sizeDelta = Vector2.one * size;
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;
    }
}
