using UnityEngine;
using UnityEngine.UI;

// Android back button: closes the open sub-menu, or quits from the main menu.
// Also hides the QUIT button on platforms where quitting is not allowed.
public class MenuBackButton : MonoBehaviour
{
    public GameObject mainMenu;
    public GameObject quitButton;
    public NamePopup namePopup;

    void Start()
    {
#if UNITY_IOS || UNITY_WEBGL
        if (quitButton != null) quitButton.SetActive(false);
#endif
        if (NamePopup.openOnLoad && namePopup != null)
        {
            NamePopup.openOnLoad = false;
            namePopup.Show();
        }
    }

    void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;

        // Top-most window first (the name window sits on top of everything)
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform panel = transform.GetChild(i);
            if (!panel.gameObject.activeSelf || panel.gameObject == mainMenu) continue;
            Button back = FindBack(panel);
            if (back != null)
            {
                back.onClick.Invoke();
                return;
            }
        }

        if (mainMenu != null && mainMenu.activeSelf)
        {
            Application.Quit();
        }
    }

    static Button FindBack(Transform root)
    {
        foreach (Button b in root.GetComponentsInChildren<Button>(false))
        {
            string n = b.name.Trim();
            if (n == "Back Button" || n == "Skip Button") return b;   // the tutorial has SKIP instead of BACK
        }
        return null;
    }
}
