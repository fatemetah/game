using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    TMPro.TMP_Text wallet;

    void OnEnable()
    {
        DooghSkins.Changed += RefreshWallet;
        RefreshWallet();
    }

    void OnDisable() { DooghSkins.Changed -= RefreshWallet; }

    void Start()
    {
        var tagline = transform.Find("Tagline")?.GetComponent<TMPro.TMP_Text>();
        if (tagline == null) return;
        wallet = Instantiate(tagline, transform);
        wallet.name = "Wallet Balance";
        wallet.fontSize = 54f;
        wallet.characterSpacing = 1f;
        var footer = wallet.rectTransform;
        footer.anchorMin = footer.anchorMax = footer.pivot = Vector2.zero;
        footer.sizeDelta = new Vector2(420f, 64f);
        footer.anchoredPosition = new Vector2(20f, 16f);
        wallet.alignment = TMPro.TextAlignmentOptions.MidlineLeft;
        wallet.enableAutoSizing = false;


        wallet.raycastTarget = false;
        RefreshWallet();
    }

    void RefreshWallet()
    {
        if (wallet != null) wallet.text = "WALLET: " + Coin_Counter.WalletBalance;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!!");
        Application.Quit();
    }
}
