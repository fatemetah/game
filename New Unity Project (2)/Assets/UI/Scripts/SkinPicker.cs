using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SkinPicker : MonoBehaviour
{
    static readonly Color Ink = new Color32(22, 23, 32, 255);
    static readonly Color Yellow = new Color32(255, 211, 0, 255);
    static readonly Dictionary<string, string> AbilityGuide = new Dictionary<string, string>
    {
        { "Classic", "Hold to fire a steady stream of shots straight up." },
        { "Mint", "Double the chance of a mint drop. Pick it up to slow enemies for 10s." },
        { "Gas", "Double the chance of a gas drop. Pick it up for 10s of rapid triple shots." },
        { "Herbs", "Double the chance of a ghost appearing. Catch one for a 10s shield." },
        { "Persian", "Plays traditional Iranian music during the game." },
        { "Ayran", "Foam spills from both sides of the cup and hits enemies below." },
        { "Water", "Sometimes fires a stream of water shots that rain back down." },
        { "Juice", "Each shot has a 20% chance to fire a seed that breaks any can or cube in one hit." },
        { "Milk", "A little milk carton visits, flies in a figure eight and fires 9 shots." }
    };
    readonly List<GameObject> hidden = new List<GameObject>();
    readonly List<Image> cards = new List<Image>();
    readonly List<GameObject> marks = new List<GameObject>();
    readonly List<GameObject> locks = new List<GameObject>();
    TMP_Text wallet, purchaseMessage;
    RectTransform confirmation;
    TMP_Text purchaseQuestion;
    Image purchasePreview;
    string pendingSkin;
    RectTransform page;
    RectTransform grid, guide;
    ScrollRect guideScroll;
    TMP_FontAsset font;
    Sprite buttonSprite;

    bool backgrounds;
    string[] Ids => backgrounds ? DooghBackgrounds.Ids : DooghSkins.Ids;
    string[] Names => backgrounds ? DooghBackgrounds.Names : DooghSkins.Names;
    CosmeticInventory Inventory => backgrounds ? DooghBackgrounds.Inventory : DooghSkins.Inventory;
    Sprite Preview(string id) => backgrounds ? DooghBackgrounds.Preview(id) : DooghSkins.Art(id);

    public void Open(TMP_FontAsset menuFont, bool showBackgrounds = false)
    {
        backgrounds = showBackgrounds;
        font = menuFont;
        if (page == null) Build();
        hidden.Clear();
        foreach (Transform child in transform)
        {
            if (child == page || !child.gameObject.activeSelf) continue;
            hidden.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
        page.gameObject.SetActive(true);
        grid.gameObject.SetActive(true);
        if (guide != null) guide.gameObject.SetActive(false);
        purchaseMessage.text = "";
        Refresh();
    }

    public bool Close()
    {
        if (page == null || !page.gameObject.activeSelf) return false;
        pendingSkin = null;
        confirmation.gameObject.SetActive(false);
        page.gameObject.SetActive(false);
        foreach (var item in hidden) if (item != null) item.SetActive(true);
        hidden.Clear();
        return true;
    }

    void OnDisable() { Close(); }

    void Build()
    {
        buttonSprite = transform.Find("Back Button").GetComponent<Image>().sprite;
        page = Rect(backgrounds ? "Background Picker" : "Skin Picker", transform, 0, 0, 0, 0);
        page.anchorMin = Vector2.zero;
        page.anchorMax = Vector2.one;
        page.offsetMin = page.offsetMax = Vector2.zero;
        page.gameObject.AddComponent<Image>().color = Ink;
        grid = Rect("Skin Selection", page, 0, 0, 0, 0);
        Text(grid, backgrounds ? "BACKGROUND" : "SKIN", 0, 525, backgrounds ? 800 : 360, 120, 96, Yellow);
        wallet = Text(grid, "Wallet", 0, 451, 650, 54, 43, Color.white);
        if (!backgrounds) MenuButton(grid, "Guide Button", "GUIDE", 315, 525, 172, 80, 44, () => ShowGuide(true));
        var chainSprite = DooghSkins.Art("ChainLock");
        for (int i = 0; i < Ids.Length; i++)
        {
            string id = Ids[i];
            var card = Rect(id, grid, (i % 3 - 1) * 275, 280 - i / 3 * 290, 253, 263);
            var bg = card.gameObject.AddComponent<Image>();
            bg.sprite = buttonSprite;
            cards.Add(bg);
            var btn = card.gameObject.AddComponent<Button>();
            btn.targetGraphic = bg;
            string displayName = Names[i];
            btn.onClick.AddListener(() => Choose(id, displayName));
            var art = Rect("Preview", card, 0, 14, 208, 158).gameObject.AddComponent<Image>();
            art.sprite = Preview(id);
            art.preserveAspect = true;
            art.raycastTarget = false;
            Text(card, Names[i], -2, -86, 205, 52, 34, Ink);
            marks.Add(Text(card, "SELECTED", 0, 103, 142, 30, 26, Ink).gameObject);
            var locked = Rect("Locked", card, 0, 0, 253, 263);
            locks.Add(locked.gameObject);
            if (id != "Classic")
            {
                var chain = Rect("Chain", locked, 0, 5, 187, 69).gameObject.AddComponent<Image>();
                chain.sprite = chainSprite;
                chain.preserveAspect = true;
                chain.raycastTarget = false;
                var tag = Rect("Price", locked, 0, 98, 150, 36);
                var priceTag = tag.gameObject.AddComponent<Image>();
                priceTag.color = Ink;
                priceTag.raycastTarget = false;
                Text(tag, "BUY  " + Inventory.Price(id), 0, 0, 144, 36, 29, Yellow);
            }
        }
        purchaseMessage = Text(grid, "", 0, -476, 810, 50, 36, Yellow);
        MenuButton(grid, "Back Button", "BACK", 0, -575, 350, 91, 54, () => Close());
        if (!backgrounds) BuildGuide();
        BuildConfirmation();
    }

    void Choose(string id, string displayName)
    {
        if (Inventory.IsOwned(id))
        {
            Inventory.Select(id);
            purchaseMessage.text = "";
            Refresh();
            return;
        }
        int price = Inventory.Price(id);
        if (Coin_Counter.WalletBalance < price)
        {
            ShowShortfall(price);
            return;
        }
        pendingSkin = id;
        purchasePreview.sprite = Preview(id);
        purchaseQuestion.text = "BUY " + displayName + "\nFOR " + price + " COINS?";
        grid.gameObject.SetActive(false);
        confirmation.gameObject.SetActive(true);
    }

    void BuildConfirmation()
    {
        confirmation = Rect("Confirm Skin Purchase", page, 0, 0, 0, 0);
        Text(confirmation, "CONFIRM PURCHASE", 0, 390, 800, 110, 76, Yellow);
        purchasePreview = Rect("Preview", confirmation, 0, 115, 330, 260).gameObject.AddComponent<Image>();
        purchasePreview.preserveAspect = true;
        purchasePreview.raycastTarget = false;
        purchaseQuestion = Text(confirmation, "Purchase Question", 0, -152, 790, 186, 60, Color.white);
        // Put the actions below all skin cards so a second tap on a card cannot confirm.
        var cancel = MenuButton(confirmation, "Back Button", "CANCEL", -166, -490, 286, 96, 47, CancelPurchase);
        cancel.GetComponent<Image>().color = Color.white;
        MenuButton(confirmation, "Buy Button", "BUY", 166, -490, 286, 96, 52, ConfirmPurchase);
        confirmation.gameObject.SetActive(false);
    }

    void CancelPurchase()
    {
        pendingSkin = null;
        purchaseMessage.text = "";
        confirmation.gameObject.SetActive(false);
        grid.gameObject.SetActive(true);
    }

    void ConfirmPurchase()
    {
        string id = pendingSkin;
        if (id == null) return;
        CancelPurchase();
        if (Inventory.TryBuyAndSelect(id))
            purchaseMessage.text = "PURCHASED AND EQUIPPED";
        else
            ShowShortfall(Inventory.Price(id));
        Refresh();
    }

    void ShowShortfall(int price)
    {
        purchaseMessage.text = "NEED " + (price - Coin_Counter.WalletBalance) + " MORE COINS";
    }

    void BuildGuide()
    {
        guide = Rect("Skin Guide", page, 0, 0, 0, 0);
        guide.anchorMin = Vector2.zero;
        guide.anchorMax = Vector2.one;
        guide.offsetMin = guide.offsetMax = Vector2.zero;
        var top = new Vector2(0.5f, 1f);
        var heading = Text(guide, "SKIN GUIDE", 0, -80, 780, 108, 86, Yellow).rectTransform;
        heading.anchorMin = heading.anchorMax = top;

        var viewport = Rect("Guide List", guide, 0, 0, 802, 0);
        viewport.anchorMin = new Vector2(0.5f, 0f);
        viewport.anchorMax = top;
        viewport.offsetMin = new Vector2(-401, 152);
        viewport.offsetMax = new Vector2(401, -149);
        viewport.gameObject.AddComponent<Image>().color = Ink;
        viewport.gameObject.AddComponent<RectMask2D>();
        guideScroll = viewport.gameObject.AddComponent<ScrollRect>();
        guideScroll.viewport = viewport;
        guideScroll.horizontal = false;
        guideScroll.vertical = true;
        guideScroll.movementType = ScrollRect.MovementType.Clamped;
        guideScroll.scrollSensitivity = 60f;
        var content = Rect("Entries", viewport, 0, 0, 802, 0);
        content.anchorMin = content.anchorMax = content.pivot = top;
        content.anchoredPosition = Vector2.zero;
        guideScroll.content = content;

        float usedHeight = 0f;
        for (int i = 0; i < Ids.Length; i++)
        {
            string id = Ids[i];
            if (i > 0) usedHeight += 14f;
            var row = Rect(id, content, 0, 0, 802, 0);
            row.anchorMin = row.anchorMax = row.pivot = top;
            row.anchoredPosition = new Vector2(0, -usedHeight);
            row.gameObject.AddComponent<Image>().color = new Color32(43, 45, 57, 255);
            var art = Rect("Preview", row, -327, 0, 110, 120).gameObject.AddComponent<Image>();
            art.sprite = Preview(id);
            art.preserveAspect = true;
            art.raycastTarget = false;
            const float textWidth = 630f;
            var title = Text(row, Names[i], 65, -16, textWidth, 65, 54, Yellow);
            title.rectTransform.anchorMin = title.rectTransform.anchorMax = title.rectTransform.pivot = top;
            title.alignment = TextAlignmentOptions.TopLeft;
            var detail = Text(row, AbilityGuide[id], 65, -83, textWidth, 0, 44, Color.white);
            detail.rectTransform.anchorMin = detail.rectTransform.anchorMax = detail.rectTransform.pivot = top;
            detail.enableWordWrapping = true;
            detail.enableAutoSizing = false;
            detail.alignment = TextAlignmentOptions.TopLeft;
            float textHeight = Mathf.Ceil(detail.GetPreferredValues(detail.text, textWidth, Mathf.Infinity).y) + 4f;
            detail.rectTransform.sizeDelta = new Vector2(textWidth, textHeight);
            float height = 83f + textHeight + 20f;
            row.sizeDelta = new Vector2(802, height);
            usedHeight += height;
        }
        content.sizeDelta = new Vector2(802, usedHeight);
        var back = MenuButton(guide, "Back Button", "BACK", 0, 76, 350, 91, 54, () => ShowGuide(false));
        back.anchorMin = back.anchorMax = new Vector2(0.5f, 0f);
    }

    void ShowGuide(bool show)
    {
        grid.gameObject.SetActive(!show);
        guide.gameObject.SetActive(show);
        if (show)
        {
            guideScroll.StopMovement();
            guideScroll.verticalNormalizedPosition = 1f;
        }
    }

    RectTransform MenuButton(Transform parent, string name, string caption, float x, float y,
        float width, float height, float size, UnityEngine.Events.UnityAction action)
    {
        var rect = Rect(name, parent, x, y, width, height);
        var image = rect.gameObject.AddComponent<Image>();
        image.sprite = buttonSprite;
        image.color = Yellow;
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(action);
        Text(rect, caption, 0, 0, width - 16, height - 8, size, Ink);
        return rect;
    }

    void Refresh()
    {
        wallet.text = "WALLET: " + Coin_Counter.WalletBalance;
        string current = Inventory.Current;
        for (int i = 0; i < cards.Count; i++)
        {
            bool selected = Ids[i] == current;
            cards[i].color = selected ? Yellow : Color.white;
            marks[i].SetActive(selected);
            locks[i].SetActive(!Inventory.IsOwned(Ids[i]));
        }
    }

    TMP_Text Text(Transform parent, string value, float x, float y, float w, float h, float size, Color color)
    {
        var label = Rect(value, parent, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
        label.font = font;
        label.text = value;
        label.fontSize = size;
        label.enableAutoSizing = true;
        label.fontSizeMin = size * 0.78f;
        label.fontSizeMax = size;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = false;
        label.color = color;
        label.raycastTarget = false;
        return label;
    }

    static RectTransform Rect(string name, Transform parent, float x, float y, float w, float h)
    {
        var rt = (RectTransform)new GameObject(name, typeof(RectTransform)).transform;
        rt.gameObject.layer = parent.gameObject.layer;
        rt.SetParent(parent, false);
        rt.sizeDelta = new Vector2(w, h);
        rt.anchoredPosition = new Vector2(x, y);
        return rt;
    }
}
