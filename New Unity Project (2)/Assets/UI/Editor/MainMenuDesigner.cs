using System.Linq;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;
using UnityEditor.Events;

// Tools > Redesign Main Menu
// Restyles the Menu scene: background, title, hero, button column, sub-menus.
// Safe to run more than once. Button click events are kept as they are.
public static class MainMenuDesigner
{
    const string UI = "Assets/UI/";
    const string ScenePath = "Assets/Scenes/Menu.unity";

    // Palette
    static readonly Color Yellow = Hex("FFD600");
    static readonly Color Ink = Hex("14141E");
    static readonly Color Red = Hex("DC1E28");
    static readonly Color White = Color.white;

    static TMP_FontAsset font;
    static Font legacyFont;
    static Material titleMat, textMat, outlineMat;
    static Sprite pillSprite, rowSprite, tabSprite, panelSprite, fieldSprite, bgSprite, btnLarge, btnMedium, btnSmall, checkSprite, tickSprite, barSprite, heroSprite, canSprite, vsSprite;

    [MenuItem("Tools/Redesign Main Menu")]
    public static void Run()
    {
        // This tool rebuilds the menus, pause/game-over screens and the tutorial from code.
        // Anything you changed by hand in those screens would be replaced - so ask first.
        if (!EditorUtility.DisplayDialog("Redesign Main Menu",
                "This rebuilds the menu, pause, game over and tutorial screens from code.\n\n" +
                "Changes you made BY HAND to those screens in Unity will be replaced.\n" +
                "(Gameplay objects, sprites and scripts are not affected.)\n\nContinue?",
                "Rebuild", "Cancel"))
            return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        Scene scene = EditorSceneManager.OpenScene(ScenePath);

        PrepareAssets();

        Transform canvas = scene.GetRootGameObjects().First(g => g.name.Trim() == "Canvas").transform;

        // Canvas scaling for phones (portrait)
        var scaler = canvas.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(900, 1600);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0f;

        // Remove the old unused duplicate menu
        Transform dup = Child(canvas, "Main Menu (1)");
        if (dup != null) Object.DestroyImmediate(dup.gameObject);

        Background(Child(canvas, "Back ground Panel"));
        BuildMainMenu(canvas);
        OptionsMenu(Child(canvas, "OptionsMenu"));
        ModeMenu(Child(canvas, "ChangeModeMenu"));
        // Player Name screen removed
        Transform nameMenu = Child(canvas, "ChangeNameMenu");
        if (nameMenu != null) Object.DestroyImmediate(nameMenu.gameObject);
        BuildLeaderboard(Child(canvas, "LeaderBoardMenu"));
        NamePopup popup = BuildNamePopup(canvas);

        // Every PLAY button now opens the name window first
        RewirePlay(Child(Child(canvas, "Main Menu"), "Play Button"), popup);
        RewirePlay(Child(Child(canvas, "ChangeModeMenu"), "Play Button"), popup);

        BuildTutorial(canvas);

        var back = Get<MenuBackButton>(canvas);
        back.mainMenu = Child(canvas, "Main Menu").gameObject;
        back.quitButton = Child(back.mainMenu.transform, "Quit Button").gameObject;
        back.namePopup = popup;

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        SetupLevelScene();
        EditorSceneManager.OpenScene(ScenePath);
        Debug.Log("Main menu redesigned.");
    }

    // ---------------------------------------------------------------- assets

    static void PrepareAssets()
    {
        bgSprite = SpriteAt(UI + "ComicBackground.png", Vector4.zero);
        btnLarge = SpriteAt(UI + "ButtonLarge.png", Vector4.zero);
        btnMedium = SpriteAt(UI + "ButtonMedium.png", Vector4.zero);
        btnSmall = SpriteAt(UI + "ButtonSmall.png", Vector4.zero);
        checkSprite = SpriteAt(UI + "CheckBox.png", Vector4.zero);
        tickSprite = SpriteAt(UI + "Tick.png", Vector4.zero);
        barSprite = SpriteAt(UI + "Bar.png", Vector4.zero);
        heroSprite = SpriteAt(UI + "DooghHero.png", Vector4.zero);
        canSprite = SpriteAt(UI + "SodaCan.png", Vector4.zero);
        vsSprite = SpriteAt(UI + "VSBadge.png", Vector4.zero);
        rowSprite = SpriteAt(UI + "Row.png", Vector4.zero);
        pillSprite = SpriteAt(UI + "Pill.png", Vector4.zero);
        tabSprite = SpriteAt(UI + "Tab.png", Vector4.zero);
        panelSprite = SpriteAt(UI + "Panel.png", Vector4.zero);
        fieldSprite = SpriteAt(UI + "Field.png", new Vector4(20, 20, 20, 20));

        legacyFont = AssetDatabase.LoadAssetAtPath<Font>(UI + "Fonts/Bangers-Regular.ttf");

        string faPath = UI + "Fonts/Bangers SDF.asset";
        font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(faPath);
        if (font == null)
        {
            font = TMP_FontAsset.CreateFontAsset(legacyFont, 90, 9, GlyphRenderMode.SDFAA, 1024, 1024,
                AtlasPopulationMode.Dynamic, true);
            font.name = "Bangers SDF";
            AssetDatabase.CreateAsset(font, faPath);
            font.material.name = "Bangers SDF Material";
            AssetDatabase.AddObjectToAsset(font.material, font);
            font.atlasTexture.name = "Bangers SDF Atlas";
            AssetDatabase.AddObjectToAsset(font.atlasTexture, font);
            font.TryAddCharacters("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789 !?.,:;-+'\"()&/%#");
            EditorUtility.SetDirty(font.atlasTexture);
            EditorUtility.SetDirty(font);
            AssetDatabase.SaveAssets();
        }

        // Persian letters come from Vazirmatn, used automatically when Bangers has no glyph
        string vzPath = UI + "Fonts/Vazirmatn SDF.asset";
        var vazir = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(vzPath);
        if (vazir == null)
        {
            var vzFont = AssetDatabase.LoadAssetAtPath<Font>(UI + "Fonts/Vazirmatn-ExtraBold.ttf");
            vazir = TMP_FontAsset.CreateFontAsset(vzFont, 90, 9, GlyphRenderMode.SDFAA, 2048, 2048,
                AtlasPopulationMode.Dynamic, true);
            vazir.name = "Vazirmatn SDF";
            AssetDatabase.CreateAsset(vazir, vzPath);
            vazir.material.name = "Vazirmatn SDF Material";
            AssetDatabase.AddObjectToAsset(vazir.material, vazir);
            vazir.atlasTexture.name = "Vazirmatn SDF Atlas";
            AssetDatabase.AddObjectToAsset(vazir.atlasTexture, vazir);
            var chars = new System.Text.StringBuilder();
            for (int c = 0xFB50; c <= 0xFBFF; c++) chars.Append((char)c);
            for (int c = 0xFE70; c <= 0xFEFC; c++) chars.Append((char)c);
            for (int c = 0x06F0; c <= 0x06F9; c++) chars.Append((char)c);
            vazir.TryAddCharacters(chars.ToString());
            EditorUtility.SetDirty(vazir.atlasTexture);
            EditorUtility.SetDirty(vazir);
        }
        var settings = TMP_Settings.instance;
        if (settings != null && TMP_Settings.fallbackFontAssets != null && !TMP_Settings.fallbackFontAssets.Contains(vazir))
        {
            TMP_Settings.fallbackFontAssets.Add(vazir);
            EditorUtility.SetDirty(settings);
        }
        if (font.fallbackFontAssetTable == null) font.fallbackFontAssetTable = new System.Collections.Generic.List<TMP_FontAsset>();
        if (!font.fallbackFontAssetTable.Contains(vazir))
        {
            font.fallbackFontAssetTable.Add(vazir);
            EditorUtility.SetDirty(font);
        }
        AssetDatabase.SaveAssets();

        titleMat = Preset("Bangers Title", m =>
        {
            m.EnableKeyword("OUTLINE_ON");
            m.SetFloat("_OutlineWidth", 0.3f);
            m.SetColor("_OutlineColor", Ink);
            m.SetFloat("_FaceDilate", 0.3f);
            m.EnableKeyword("UNDERLAY_ON");
            m.SetColor("_UnderlayColor", Ink);
            m.SetFloat("_UnderlayOffsetX", 0.6f);
            m.SetFloat("_UnderlayOffsetY", -0.6f);
            m.SetFloat("_UnderlaySoftness", 0f);
            m.SetFloat("_UnderlayDilate", 0.6f);
        });

        outlineMat = Preset("Bangers Outline", m =>
        {
            m.EnableKeyword("OUTLINE_ON");
            m.SetFloat("_OutlineWidth", 0.32f);
            m.SetColor("_OutlineColor", Ink);
            m.SetFloat("_FaceDilate", 0.32f);
        });

        textMat = Preset("Bangers Plain", m => { });

        AssetDatabase.SaveAssets();
    }

    static Sprite SpriteAt(string path, Vector4 border)
    {
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.textureType = TextureImporterType.Sprite;
        ti.spriteImportMode = SpriteImportMode.Single;
        ti.mipmapEnabled = false;
        ti.alphaIsTransparency = true;
        ti.spriteBorder = border;
        ti.filterMode = FilterMode.Bilinear;
        ti.maxTextureSize = 4096;   // the tall kitchen picture is 1080x2520
        ti.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Material Preset(string name, System.Action<Material> setup)
    {
        string path = UI + "Fonts/" + name + ".mat";
        var m = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (m == null)
        {
            m = new Material(font.material) { name = name };
            AssetDatabase.CreateAsset(m, path);
        }
        else
        {
            m.CopyPropertiesFromMaterial(font.material);
        }
        m.shaderKeywords = new string[0];
        setup(m);
        EditorUtility.SetDirty(m);
        return m;
    }

    // ---------------------------------------------------------------- screens

    static void Background(Transform bg)
    {
        if (bg == null) return;
        bg.SetAsFirstSibling();
        Stretch(bg);
        var img = Get<Image>(bg);
        img.sprite = bgSprite;
        img.type = Image.Type.Simple;
        img.color = Color.white;
        img.preserveAspect = false;
        img.raycastTarget = false;
        var fit = Get<AspectRatioFitter>(bg);
        fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fit.aspectRatio = 1080f / 1920f;
    }

    static void BuildMainMenu(Transform canvas)
    {
        Transform menu = Child(canvas, "Main Menu");
        Stretch(menu);
        Fade(menu);

        Transform title = Child(menu, "Title") ?? NewText(menu, "Title");
        title.SetSiblingIndex(0);
        Place(title, 640, 900, 230);
        title.localRotation = Quaternion.Euler(0, 0, 4);
        var tt = StyleTMP(title, 180, Yellow, titleMat);
        tt.text = "SUGAR RUSH";
        tt.characterSpacing = 4;

        Transform tag = Child(menu, "Tagline") ?? NewText(menu, "Tagline");
        tag.SetSiblingIndex(1);
        Place(tag, 500, 860, 70);
        var tg = StyleTMP(tag, 54, White, outlineMat);
        tg.text = "THE HEALTHY HERO STRIKES BACK";
        tg.characterSpacing = 3;

        Transform hero = Picture(menu, "Hero", heroSprite, -226, 185, 372, 495, 2);
        var hf = Get<UIFloat>(hero); hf.amplitude = 16; hf.speed = 1.4f; hf.tilt = 3;

        Transform can = Picture(menu, "SodaCan", canSprite, 320, -29, 279, 413, 3);
        var cf = Get<UIFloat>(can); cf.amplitude = 10; cf.speed = 1.1f; cf.tilt = 4;

        Transform vs = Picture(menu, "VSBadge", vsSprite, 0, -17, 258, 258, 4);
        Get<UIButtonFX>(vs).pulse = true;

        StyleButton(Child(menu, "Play Button"), "PLAY", btnLarge, Yellow, Ink, -284, 612, 170, 117, true);
        StyleButton(Child(menu, "Options Button"), "OPTIONS", btnMedium, White, Ink, -455, 520, 121, 69);
        StyleButton(Child(menu, "Leaderboard"), "SCORES", btnMedium, White, Ink, -593, 520, 121, 69);
        StyleButton(Child(menu, "Quit Button"), "QUIT", btnSmall, Ink, White, -718, 383, 92, 54);
    }

    static Transform Picture(Transform parent, string name, Sprite sprite, float x, float y, float w, float h, int index)
    {
        Transform t = Child(parent, name);
        if (t == null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.layer = parent.gameObject.layer;
            t = go.transform;
            t.SetParent(parent, false);
        }
        t.SetSiblingIndex(index);
        Place(t, y, w, h);
        ((RectTransform)t).anchoredPosition = new Vector2(x, y);
        var img = t.GetComponent<Image>();
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = true;
        img.color = Color.white;
        img.raycastTarget = false;
        return t;
    }

    static void OptionsMenu(Transform panel)
    {
        if (panel == null) return;
        Stretch(panel);
        Fade(panel);
        PanelTitle(Child(panel, "Options Text"), "OPTIONS");

        Transform vt = Child(panel, "Volume Text");
        Place(vt, 340, 600, 80);
        var v = StyleTMP(vt, 66, White, outlineMat);
        if (v != null) v.text = "VOLUME";

        Transform slider = Child(panel, "Volume Slider");
        Place(slider, 250, 600, 46);
        Tint(Child(slider, "Background"), barSprite, Ink);
        Tint(Deep(slider, "Fill"), barSprite, Yellow);
        Tint(Deep(slider, "Handle"), checkSprite, White);
        var hr = (RectTransform)Deep(slider, "Handle");
        if (hr != null) hr.sizeDelta = new Vector2(70, 34);

        VibrationRow(panel);

        StyleButton(Child(panel, "ChangMode Button"), "GAME MODE", btnMedium, White, Ink, -170, 520, 121, 66);
        var gmRt = (RectTransform)Child(panel, "ChangMode Button");
        if (gmRt != null) gmRt.anchoredPosition = new Vector2(-8f, -170f);   // the hard shadow sits on the right, so nudge left to look centred
        Transform nameBtn = Child(panel, "ChangName Button");
        if (nameBtn != null) Object.DestroyImmediate(nameBtn.gameObject);
        StyleButton(Child(panel, "Back Button"), "BACK", btnSmall, Ink, White, -600, 383, 92, 54);
    }

    // OPTIONS > VIBRATION: a copy of one of the game-mode tick boxes, rewired to the vibration setting
    static void VibrationRow(Transform panel)
    {
        Transform old = Child(panel, "Vibration Toggle");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        Transform canvas = panel.parent;
        Transform modeMenu = Child(canvas, "ChangeModeMenu");
        Toggle source = modeMenu != null ? modeMenu.GetComponentInChildren<Toggle>(true) : null;
        if (source == null) { Debug.LogWarning("No tick box found to copy for VIBRATION"); return; }

        Transform t = Object.Instantiate(source.gameObject, panel).transform;
        t.name = "Vibration Toggle";
        t.gameObject.SetActive(true);
        var tg = t.GetComponent<Toggle>();
        tg.group = null;
        for (int i = tg.onValueChanged.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(tg.onValueChanged, i);
        foreach (MonoBehaviour mb in t.GetComponents<MonoBehaviour>())
            if (!(mb is Toggle) && !(mb is Image) && !(mb is UIButtonFX)) Object.DestroyImmediate(mb);
        tg.isOn = true;
        ToggleRow(t, "VIBRATION", 30);
        t.gameObject.AddComponent<VibrationToggle>();
    }

    static void ModeMenu(Transform panel)
    {
        if (panel == null) return;
        Stretch(panel);
        Fade(panel);
        PanelTitle(Child(panel, "ChangeMode Text"), "GAME MODE");

        Transform group = Child(panel, "Modes Toggle");
        Place(group, 0, 900, 1000);

        string[] names = { "EasyMode Toggle", "MediumMode Toggle", "HardMode Toggle", "TahmaMode Toggle" };
        string[] labels = { "EASY", "MEDIUM", "HARD", "TAHMA!!" };
        for (int i = 0; i < names.Length; i++)
        {
            Transform t = Child(group, names[i]);
            if (t != null) ToggleRow(t, labels[i], 300 - i * 140);
        }

        // No Save button: the mode is saved as soon as it is ticked
        Transform submit = Child(group, "Submit Button");
        if (submit != null) Object.DestroyImmediate(submit.gameObject);

        var tg = group.GetComponent<ToggleGroup>();
        if (tg != null) tg.allowSwitchOff = false;

        // PLAY button here too, so the player can start right after picking a mode
        Transform play = Child(panel, "Play Button");
        if (play == null)
        {
            Transform mainPlay = Child(Child(panel.parent, "Main Menu"), "Play Button");
            if (mainPlay != null)
            {
                play = Object.Instantiate(mainPlay.gameObject, panel).transform;
                play.name = "Play Button";
            }
        }
        StyleButton(play, "PLAY", btnLarge, Yellow, Ink, -330, 560, 155, 105, true);
        StyleButton(Child(panel, "Back Button"), "BACK", btnSmall, Ink, White, -600, 383, 92, 54);
    }

    static void SimpleMenu(Transform panel, string titleName, string title)
    {
        if (panel == null) return;
        Stretch(panel);
        Fade(panel);
        PanelTitle(Child(panel, titleName), title);

        Transform info = Child(panel, "Info") ?? NewText(panel, "Info");
        Place(info, 120, 760, 200);
        var it = StyleTMP(info, 80, White, outlineMat);
        it.text = "COMING SOON!";

        StyleButton(Child(panel, "Back Button"), "BACK", btnSmall, Ink, White, -600, 383, 92, 54);
    }

    // ---------------------------------------------------------------- widgets

    static void PanelTitle(Transform t, string text)
    {
        if (t == null) return;
        Place(t, 600, 900, 220);
        t.localRotation = Quaternion.Euler(0, 0, 3);
        var tx = StyleTMP(t, 160, Yellow, titleMat);
        if (tx == null) return;
        tx.text = text;
        tx.characterSpacing = 4;
    }

    static void StyleButton(Transform b, string label, Sprite sprite, Color fill, Color textColor,
        float y, float w, float h, float size, bool pulse = false)
    {
        if (b == null) return;
        Place(b, y, w, h);

        var img = Get<Image>(b);
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.color = fill;

        var btn = b.GetComponent<Button>();
        if (btn != null)
        {
            btn.targetGraphic = img;
            btn.transition = Selectable.Transition.ColorTint;
            var cb = btn.colors;
            cb.normalColor = Color.white;
            cb.highlightedColor = Color.white;
            cb.selectedColor = Color.white;
            cb.pressedColor = new Color(0.85f, 0.85f, 0.85f, 1f);
            cb.colorMultiplier = 1f;
            cb.fadeDuration = 0.08f;
            btn.colors = cb;
        }

        // The old soft shadow is not needed: the sprite has a hard comic shadow baked in.
        var sh = b.GetComponent<Shadow>();
        if (sh != null) Object.DestroyImmediate(sh);

        Get<UIButtonFX>(b).pulse = pulse;

        Transform txt = b.GetComponentsInChildren<TMP_Text>(true).Select(x => x.transform).FirstOrDefault();
        if (txt != null)
        {
            var r = (RectTransform)txt;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.pivot = new Vector2(0.5f, 0.5f);
            float shadow = h * 0.07f;
            r.offsetMin = new Vector2(0f, shadow);
            r.offsetMax = new Vector2(-shadow, 0f);
            r.localScale = Vector3.one;
            r.localRotation = Quaternion.identity;
            var tx = StyleTMP(txt, size, textColor, textMat);
            tx.text = label;
            tx.characterSpacing = 4;
        }
    }

    static void ToggleRow(Transform t, string label, float y)
    {
        Place(t, y, 520, 121);

        var row = Get<Image>(t);
        row.sprite = btnMedium;
        row.type = Image.Type.Simple;
        row.preserveAspect = false;
        row.color = White;

        Transform box = Child(t, "Background");
        if (box != null)
        {
            var r = (RectTransform)box;
            r.anchorMin = r.anchorMax = new Vector2(0f, 0.5f);
            r.pivot = new Vector2(0f, 0.5f);
            r.anchoredPosition = new Vector2(52f, 4f);
            r.sizeDelta = new Vector2(64f, 64f);
            Tint(box, checkSprite, White);

            Transform check = Child(box, "Checkmark");
            if (check != null)
            {
                var c = (RectTransform)check;
                c.anchorMin = Vector2.zero;
                c.anchorMax = Vector2.one;
                c.offsetMin = new Vector2(4f, 4f);
                c.offsetMax = new Vector2(-4f, -4f);
                Tint(check, tickSprite, Color.white);
            }
        }

        Transform lbl = Child(t, "Label");
        if (lbl != null)
        {
            var r = (RectTransform)lbl;
            r.anchorMin = Vector2.zero;
            r.anchorMax = Vector2.one;
            r.offsetMin = new Vector2(140f, 8f);
            r.offsetMax = new Vector2(-30f, 0f);
            StyleAny(lbl, 66, Ink, label, left: true);
        }
    }


    // ---------------------------------------------------------------- name + scores

    static void RewirePlay(Transform play, NamePopup popup)
    {
        if (play == null) return;
        var btn = play.GetComponent<Button>();
        for (int i = btn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(btn.onClick, i);
        UnityEventTools.AddVoidPersistentListener(btn.onClick, popup.Show);
    }

    static NamePopup BuildNamePopup(Transform canvas)
    {
        Transform root = Child(canvas, "NamePopup");
        if (root != null) Object.DestroyImmediate(root.gameObject);

        var go = new GameObject("NamePopup", typeof(RectTransform), typeof(Image));
        go.layer = canvas.gameObject.layer;
        root = go.transform;
        root.SetParent(canvas, false);
        root.SetAsLastSibling();
        Stretch(root);
        var dim = go.GetComponent<Image>();
        dim.color = new Color(0f, 0f, 0f, 0.65f);
        dim.raycastTarget = true;
        Fade(root);

        Transform card = Picture(root, "Card", panelSprite, 0, 60, 760, 620, 0);
        card.GetComponent<Image>().preserveAspect = false;
        card.GetComponent<Image>().raycastTarget = true;

        Transform title = NewText(card, "Title");
        Place(title, 190, 760, 150);
        title.localRotation = Quaternion.Euler(0, 0, 3);
        var tt = StyleTMP(title, 110, Yellow, titleMat);
        tt.text = "YOUR NAME";
        tt.characterSpacing = 4;

        // Name box (a button: tapping it opens the phone keyboard)
        var fieldGo = new GameObject("NameInput", typeof(RectTransform), typeof(Image), typeof(Button));
        fieldGo.layer = card.gameObject.layer;
        Transform field = fieldGo.transform;
        field.SetParent(card, false);
        Place(field, 20, 600, 130);
        var fimg = fieldGo.GetComponent<Image>();
        fimg.sprite = fieldSprite;
        fimg.type = Image.Type.Sliced;
        fimg.color = Color.white;
        var fbtn = fieldGo.GetComponent<Button>();
        fbtn.transition = Selectable.Transition.None;

        Transform ph = NewText(field, "Placeholder");
        Stretch(ph);
        var pht = StyleTMP(ph, 66, new Color(0.55f, 0.55f, 0.6f, 1f), textMat);
        pht.text = "TYPE YOUR NAME";
        pht.characterSpacing = 3;

        Transform disp = NewText(field, "Display");
        Stretch(disp);
        var dt = StyleTMP(disp, 76, Ink, textMat);
        dt.richText = false;
        dt.characterSpacing = 2;

        // GO button: copy of the main PLAY button so it keeps the same look
        Transform mainPlay = Child(Child(canvas, "Main Menu"), "Play Button");
        Transform go2 = Object.Instantiate(mainPlay.gameObject, card).transform;
        go2.name = "Go Button";
        StyleButton(go2, "GO!", btnLarge, Yellow, Ink, -170, 440, 130, 96, true);

        // BACK closes the window (also used by the phone's back key)
        Transform back = Object.Instantiate(Child(Child(canvas, "OptionsMenu"), "Back Button").gameObject, root).transform;
        back.name = "Back Button";
        StyleButton(back, "BACK", btnSmall, Ink, White, -360, 383, 92, 54);

        var popup = go.AddComponent<NamePopup>();
        popup.card = (RectTransform)card;
        popup.display = dt;
        popup.placeholder = ph.gameObject;
        UnityEventTools.AddVoidPersistentListener(fbtn.onClick, popup.OpenKeyboard);

        var goBtn = go2.GetComponent<Button>();
        for (int i = goBtn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(goBtn.onClick, i);
        UnityEventTools.AddVoidPersistentListener(goBtn.onClick, popup.Confirm);

        var backBtn = back.GetComponent<Button>();
        for (int i = backBtn.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(backBtn.onClick, i);
        UnityEventTools.AddVoidPersistentListener(backBtn.onClick, popup.Hide);

        go.SetActive(false);
        return popup;
    }

    static void BuildLeaderboard(Transform panel)
    {
        if (panel == null) return;
        Stretch(panel);
        Fade(panel);
        PanelTitle(Child(panel, "LeaderBoardText"), "SCORES");

        Transform info = Child(panel, "Info");
        if (info != null) Object.DestroyImmediate(info.gameObject);
        foreach (string n in new[] { "Tabs", "Rows", "Empty" })
        {
            Transform old = Child(panel, n);
            if (old != null) Object.DestroyImmediate(old.gameObject);
        }

        var view = Get<LeaderboardView>(panel);

        // Tabs
        var tabsGo = new GameObject("Tabs", typeof(RectTransform));
        tabsGo.layer = panel.gameObject.layer;
        tabsGo.transform.SetParent(panel, false);
        Place(tabsGo.transform, 420, 900, 100);
        string[] labels = { "EASY", "MEDIUM", "HARD", "TAHMA!!" };
        view.tabImages = new Image[4];
        view.tabTexts = new TMP_Text[4];
        Transform template = Child(panel, "Back Button");
        for (int i = 0; i < 4; i++)
        {
            Transform tab = Object.Instantiate(template.gameObject, tabsGo.transform).transform;
            tab.name = labels[i] + " Tab";
            StyleButton(tab, labels[i], tabSprite, White, Ink, 0, 205, 90, 40);
            ((RectTransform)tab).anchoredPosition = new Vector2(-330 + i * 220, 0);
            var b = tab.GetComponent<Button>();
            for (int k = b.onClick.GetPersistentEventCount() - 1; k >= 0; k--)
                UnityEventTools.RemovePersistentListener(b.onClick, k);
            UnityEventTools.AddIntPersistentListener(b.onClick, view.Show, i);
            view.tabImages[i] = tab.GetComponent<Image>();
            view.tabTexts[i] = tab.GetComponentInChildren<TMP_Text>(true);
        }

        // Rows
        var rowsGo = new GameObject("Rows", typeof(RectTransform));
        rowsGo.layer = panel.gameObject.layer;
        rowsGo.transform.SetParent(panel, false);
        Place(rowsGo.transform, 0, 900, 1000);
        int count = 10;
        view.rows = new GameObject[count];
        view.rowImages = new Image[count];
        view.rowNames = new TMP_Text[count];
        view.rowScores = new TMP_Text[count];
        for (int i = 0; i < count; i++)
        {
            Transform row = Picture(rowsGo.transform, "Row " + (i + 1), rowSprite, 0, 290 - i * 80, 720, 70, i);
            row.GetComponent<Image>().preserveAspect = false;

            Transform n = NewText(row, "Name");
            var nr = (RectTransform)n;
            nr.anchorMin = Vector2.zero; nr.anchorMax = Vector2.one;
            nr.offsetMin = new Vector2(40, 6); nr.offsetMax = new Vector2(-200, 0);
            var nt = StyleTMP(n, 44, Ink, textMat);
            nt.alignment = TextAlignmentOptions.MidlineLeft;
            nt.characterSpacing = 3;
            nt.text = (i + 1) + ".  PLAYER";

            Transform sc = NewText(row, "Score");
            var sr = (RectTransform)sc;
            sr.anchorMin = Vector2.zero; sr.anchorMax = Vector2.one;
            sr.offsetMin = new Vector2(400, 6); sr.offsetMax = new Vector2(-50, 0);
            var st = StyleTMP(sc, 48, Ink, textMat);
            st.alignment = TextAlignmentOptions.MidlineRight;
            st.text = "0";

            view.rows[i] = row.gameObject;
            view.rowImages[i] = row.GetComponent<Image>();
            view.rowNames[i] = nt;
            view.rowScores[i] = st;
        }

        Transform empty = NewText(panel, "Empty");
        Place(empty, 60, 800, 260);
        var et = StyleTMP(empty, 72, White, outlineMat);
        view.emptyText = et;
        et.text = "NO SCORES YET\n<size=70%>BE THE FIRST!</size>";

        StyleButton(Child(panel, "Back Button"), "BACK", btnSmall, Ink, White, -600, 383, 92, 54);
    }

    // Level_1: lose screen gets a CHANGE NAME button and a real top-10 list
    static void SetupLevelScene()
    {
        Scene level = EditorSceneManager.OpenScene("Assets/Scenes/Level_1.unity");
        Transform loserCanvas = level.GetRootGameObjects().First(g => g.name.Trim() == "LoserMenu Canvas").transform;
        var loser = loserCanvas.GetComponent<LoserMenu>();
        Transform panel = Child(loserCanvas, "LoserMenu Panel");
        Transform main = Child(panel, "MainLoserMenu");

        Transform restart = Child(main, "Restart Button");
        Transform scores = Child(main, "Scores Button");
        Transform toMenu = Child(main, "GoToMenu Button");

        Transform change = Child(main, "ChangeName Button");
        if (change == null)
        {
            change = Object.Instantiate(toMenu.gameObject, main).transform;
            change.name = "ChangeName Button";
        }
        var rt = (RectTransform)change;
        var menuRt = (RectTransform)toMenu;
        float step = ((RectTransform)scores).anchoredPosition.y - ((RectTransform)restart).anchoredPosition.y;
        rt.anchoredPosition = menuRt.anchoredPosition + new Vector2(0, step);
        rt.sizeDelta = new Vector2(Mathf.Max(menuRt.sizeDelta.x, 240f), menuRt.sizeDelta.y);
        var label = change.GetComponentInChildren<TMP_Text>(true);
        if (label != null)
        {
            label.text = "CHANGE NAME";
            label.enableWordWrapping = false;
        }
        var b = change.GetComponent<Button>();
        for (int i = b.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(b.onClick, i);
        UnityEventTools.AddVoidPersistentListener(b.onClick, loser.ChangeName);

        Transform scoresMenu = Child(panel, "LoserScoresMenu");
        Transform scoresText = Child(scoresMenu, "Scores Text");
        if (scoresText != null) Get<LoserScoresText>(scoresText);

        // The BACK buttons of the lose-screen scores and the pause options were outside the screen
        Transform pauseCanvas = level.GetRootGameObjects().First(g => g.name.Trim() == "PauseMenu Canvas").transform;
        Transform pauseOptions = Child(Child(pauseCanvas, "PauseMenu Panel"), "OptionsMenu");
        foreach (Transform backBtn in new[] { Child(scoresMenu, "Back Button"), Child(pauseOptions, "Back Button") })
        {
            if (backBtn == null) continue;
            var br = (RectTransform)backBtn;
            br.anchorMin = br.anchorMax = new Vector2(0.5f, 0.5f);
            br.pivot = new Vector2(0.5f, 0.5f);
            br.anchoredPosition = new Vector2(0f, backBtn.parent == scoresMenu ? -240f : -150f);
            br.sizeDelta = new Vector2(200f, 40f);
            var bl = backBtn.GetComponentInChildren<TMP_Text>(true);
            if (bl != null)
            {
                bl.text = "BACK";
                bl.enableWordWrapping = false;
            }
        }

        // Game background: kitchen lunch table
        GameObject bgObj = level.GetRootGameObjects().FirstOrDefault(g => g.name.Trim() == "BackGround");
        if (bgObj != null)
        {
            var bgSr = bgObj.GetComponent<SpriteRenderer>();
            bgSr.sprite = SpriteAt(UI + "KitchenTable.png", Vector4.zero);
            bgSr.color = Color.white;
            bgSr.sortingOrder = -100;
            bgObj.transform.rotation = Quaternion.identity;
            if (bgObj.GetComponent<FitBackgroundToCamera>() == null) bgObj.AddComponent<FitBackgroundToCamera>();
            if (bgObj.GetComponent<WallClock>() == null) bgObj.AddComponent<WallClock>();   // real-time clock hands
        }

        // Taller phones: keep the whole width of the game on screen
        foreach (GameObject g in level.GetRootGameObjects())
        {
            var cam = g.GetComponentInChildren<Camera>(true);
            if (cam != null && cam.orthographic && cam.GetComponent<FitCameraWidth>() == null)
                cam.gameObject.AddComponent<FitCameraWidth>();
        }

        // Gameplay doogh bottle: full quality + real transparency (no black box, not blurry)
        SpriteAt("Assets/AmirGraphics/ShootAli21.png", Vector4.zero);

        RaiseDoogh(level);
        BuildPauseButton(level);
        BuildScoreBadge(level);
        StylePauseMenu(level);
        BuildGameOver(level, loser);

        EditorSceneManager.MarkSceneDirty(level);
        EditorSceneManager.SaveScene(level);
    }


    // The bottle used to stick out below the bottom of the screen: lift it so it is fully visible
    static void RaiseDoogh(Scene level)
    {
        GameObject shooter = level.GetRootGameObjects().FirstOrDefault(g => g.name.Trim() == "Shooter");
        Camera cam = null;
        foreach (GameObject g in level.GetRootGameObjects())
        {
            cam = g.GetComponentInChildren<Camera>(true);
            if (cam != null) break;
        }
        if (shooter == null || cam == null || !cam.orthographic) return;

        float minY = float.PositiveInfinity;
        foreach (SpriteRenderer sr in shooter.GetComponentsInChildren<SpriteRenderer>(true))
            if (sr.sprite != null) minY = Mathf.Min(minY, sr.bounds.min.y);
        if (float.IsInfinity(minY)) return;

        // Bottom of the bottle just above the bottom edge of the screen, low enough to touch the coins on the floor
        float screenBottom = cam.transform.position.y - cam.orthographicSize;
        float gap = (screenBottom + 0.05f) - minY;
        if (Mathf.Abs(gap) > 0.01f)
        {
            Undo.RecordObject(shooter.transform, "Place doogh");
            shooter.transform.position += new Vector3(0f, gap, 0f);
            EditorUtility.SetDirty(shooter.transform);
            Debug.Log("Doogh moved by " + gap.ToString("0.00"));
        }
    }

    // ---------------------------------------------------------------- remove files the game no longer uses

    static readonly string[] Unused =
    {
        "Assets/FatemehScripts/LevelsScripts/Player.cs",
        "Assets/FatemehScripts/LevelsScripts/Bullet.cs",
        "Assets/AmirScripts/WheelRotation.cs",
        "Assets/Player.prefab",
        "Assets/Bullet.prefab",
        "Assets/FatemehGraphics/Enemy 1.0.jpg",
        "Assets/FatemehGraphics/Enemy 2.0.jpg",
        "Assets/FatemehGraphics/Enemy 3.0.jpg",
        "Assets/UI/MenuBackground.png",
        "Assets/UI/MenuHero.png",
        "Assets/UI/Fonts/LilitaOne-Regular.ttf",
        "Assets/UI/Fonts/LilitaOne SDF.asset",
        "Assets/UI/Fonts/LilitaOne Shadow.mat",
        "Assets/UI/Fonts/LilitaOne Title.mat",
    };

    [MenuItem("Tools/Clean Unused Files")]
    static void CleanUnused()
    {
        var found = Unused.Where(p => AssetDatabase.LoadMainAssetAtPath(p) != null).ToList();
        if (found.Count == 0) { Debug.Log("Nothing to clean."); return; }

        // Safety: never remove something that is still used by a scene or prefab
        var used = new HashSet<string>(AssetDatabase.GetDependencies(
            AssetDatabase.FindAssets("t:Scene t:Prefab").Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => !found.Contains(p)).ToArray(), true));
        var safe = found.Where(p => !used.Contains(p)).ToList();
        var kept = found.Where(p => used.Contains(p)).ToList();

        if (!EditorUtility.DisplayDialog("Clean Unused Files",
                "Move these " + safe.Count + " unused files to the Recycle Bin?\n\n" + string.Join("\n", safe) +
                (kept.Count > 0 ? "\n\nKept (still used): " + string.Join(", ", kept) : ""),
                "Move to Recycle Bin", "Cancel"))
            return;

        foreach (string p in safe) AssetDatabase.MoveAssetToTrash(p);
        AssetDatabase.Refresh();
        Debug.Log("Moved " + safe.Count + " unused files to the Recycle Bin." + (kept.Count > 0 ? " Kept: " + string.Join(", ", kept) : ""));
    }

    // ---------------------------------------------------------------- first-time tutorial

    // ---- testing helpers (Play mode only): drop a prize right away instead of waiting for a rare one
    [MenuItem("Tools/Test: Drop Mint")]
    static void DropMint() { DropPrize(PowerUpItem.Kind.Mint); }

    [MenuItem("Tools/Test: Drop CO2")]
    static void DropGas() { DropPrize(PowerUpItem.Kind.Gas); }

    static void DropPrize(PowerUpItem.Kind kind)
    {
        if (!Application.isPlaying || Camera.main == null) { Debug.Log("Start a game first (Play mode, in the level)."); return; }
        Camera cam = Camera.main;
        PowerUpItem.Create(kind, new Vector3(0f, cam.transform.position.y + cam.orthographicSize - 1f, 0f));
    }

    [MenuItem("Tools/Show Tutorial Again")]
    static void ResetTutorial()
    {
        PlayerPrefs.DeleteKey(TutorialView.PrefKey);
        PlayerPrefs.Save();
        Debug.Log("Tutorial will show again on the next Play.");
    }

    static Sprite GameSprite(string path)
    {
        Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (s == null)
            foreach (Object o in AssetDatabase.LoadAllAssetsAtPath(path))
                if (o is Sprite) { s = (Sprite)o; break; }
        return s;
    }

    static void At(Transform t, float x, float y, float w, float h)
    {
        Place(t, y, w, h);
        ((RectTransform)t).anchoredPosition = new Vector2(x, y);
    }

    static TMP_Text Body(Transform parent, string name, string text, float y, float h, float size)
    {
        Transform t = NewText(parent, name);
        Place(t, y, 680, h);
        var tx = StyleTMP(t, size, Ink, textMat);
        tx.text = text;
        tx.enableWordWrapping = true;
        tx.lineSpacing = 8;
        tx.alignment = TextAlignmentOptions.Center;
        return tx;
    }

    static void Icon(Transform parent, string name, Sprite s, float x, float y, float h, float rot = 0)
    {
        if (s == null) return;
        float w = h * s.rect.width / s.rect.height;
        Transform t = Picture(parent, name, s, x, y, w, h, 99);
        t.GetComponent<Image>().preserveAspect = true;
        t.GetComponent<Image>().raycastTarget = false;
        t.localRotation = Quaternion.Euler(0, 0, rot);
    }

    static Transform Page(Transform card, string name, string title)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.layer = card.gameObject.layer;
        go.transform.SetParent(card, false);
        Stretch(go.transform);
        Transform tt = NewText(go.transform, "Title");
        Place(tt, 500, 760, 160);
        tt.localRotation = Quaternion.Euler(0, 0, 3);
        var tx = StyleTMP(tt, 120, Yellow, titleMat);
        tx.text = title;
        tx.characterSpacing = 4;
        return go.transform;
    }

    static void Row(Transform page, string label, Sprite sprite, Color fill, Color textColor, string desc, float y)
    {
        Transform b = NewButton(page, label + " Sample");
        Object.DestroyImmediate(b.GetComponent<Button>());
        var img = b.GetComponent<Image>();
        img.sprite = sprite; img.color = fill; img.raycastTarget = false; img.type = Image.Type.Simple;
        At(b, -215, y, 270, 96);
        var lt = StyleTMP(Child(b, "Text"), 50, textColor, textMat);
        lt.text = label;
        Transform d = NewText(page, label + " Info");
        At(d, 150, y, 420, 110);
        var dt = StyleTMP(d, 38, Ink, textMat);
        dt.text = desc;
        dt.enableWordWrapping = true;
        dt.alignment = TextAlignmentOptions.MidlineLeft;
    }

    static void BuildTutorial(Transform canvas)
    {
        Transform old = Child(canvas, "Tutorial");
        if (old != null) Object.DestroyImmediate(old.gameObject);

        var go = new GameObject("Tutorial", typeof(RectTransform), typeof(Image));
        go.layer = canvas.gameObject.layer;
        Transform root = go.transform;
        root.SetParent(canvas, false);
        root.SetAsLastSibling();
        Stretch(root);
        var dim = go.GetComponent<Image>();
        dim.color = new Color(0.04f, 0.03f, 0.08f, 0.78f);
        dim.raycastTarget = true;

        Transform card = Picture(root, "Card", panelSprite, 0, 10, 820, 1300, 0);
        card.GetComponent<Image>().preserveAspect = false;
        card.GetComponent<Image>().raycastTarget = true;

        Sprite sugar = GameSprite("Assets/FatemehGraphics/Enemy 1.0.png");
        Sprite zero = GameSprite("Assets/FatemehGraphics/Enemy 2.0.png");
        Sprite red = GameSprite("Assets/FatemehGraphics/Enemy 3.0.png");
        Sprite coin = GameSprite("Assets/AmirGraphics/Coin3.png");
        Sprite ghost = GameSprite("Assets/AmirGraphics/Ghosti.png");
        Sprite pause = GameSprite(UI + "PauseButton.png");

        var pages = new System.Collections.Generic.List<GameObject>();

        // 1. Story
        Transform p = Page(card, "Page Welcome", "WELCOME!");
        Icon(p, "Hero", heroSprite, -150, 230, 300);
        Icon(p, "VS", vsSprite, 40, 200, 130);
        Icon(p, "Can", red, 190, 220, 230, -8);
        Body(p, "Text", "YOU ARE DOOGH,\nTHE HEALTHY HERO!\n\nSUGARY SODAS AND SUGAR CUBES ARE TAKING OVER THE DINNER TABLE.\nSTOP THEM!", -170, 480, 48);
        pages.Add(p.gameObject);

        // 2. Controls
        p = Page(card, "Page Controls", "CONTROLS");
        Body(p, "Text", "DRAG LEFT & RIGHT\nTO MOVE THE DOOGH\n\nTAP TO SHOOT\n\nHOLD YOUR FINGER DOWN\nFOR NON-STOP FIRE", 150, 560, 50);
        Icon(p, "Pause", pause, -230, -260, 120);
        Transform pt = NewText(p, "Pause Info");
        At(pt, 90, -260, 480, 130);
        var ptx = StyleTMP(pt, 42, Ink, textMat);
        ptx.text = "THIS BUTTON IN THE CORNER PAUSES THE GAME";
        ptx.enableWordWrapping = true;
        ptx.alignment = TextAlignmentOptions.MidlineLeft;
        pages.Add(p.gameObject);

        // 3. Score
        p = Page(card, "Page Score", "SCORE");
        Icon(p, "Red", red, -240, 290, 180, -10);
        Icon(p, "Zero", zero, -95, 290, 180, 8);
        Icon(p, "Sugar", sugar, 70, 285, 130);
        Icon(p, "Coin", coin, 235, 285, 130);
        // how many bullets each one takes
        string[] hits = { "4 HITS", "3 HITS", "2 HITS", "= 1 POINT" };
        float[] hx = { -240, -95, 70, 235 };
        for (int i = 0; i < 4; i++)
        {
            Transform h = NewText(p, "Hits " + i);
            At(h, hx[i], 165, 170, 50);
            var ht = StyleTMP(h, 38, i == 3 ? Hex("C89B00") : Red, textMat);
            ht.text = hits[i];
        }
        Body(p, "Text", "RED CANS ARE THE TOUGHEST - EVERY HIT DENTS THEM MORE.\nDESTROYED ONES DROP COINS:\nCATCH THEM FOR POINTS!\n\nA COIN DISAPPEARS 3 SECONDS AFTER IT LANDS.\n\nIF A CAN OR A SUGAR CUBE TOUCHES YOU - GAME OVER!", -150, 440, 37);
        pages.Add(p.gameObject);

        // 4. Ghost
        p = Page(card, "Page Prizes", "PRIZES");
        Sprite mintS = GameSprite("Assets/Resources/PowerUps/Mint.png");
        Sprite gasS = GameSprite("Assets/Resources/PowerUps/Gas.png");
        string[] prizeText = {
            "GHOST: NOTHING CAN HURT YOU FOR 10 SECONDS",
            "MINT: EVERYTHING FREEZES AND SLOWS DOWN FOR 10 SECONDS",
            "CO2: THE DOOGH FIZZES OVER - TRIPLE, SUPER FAST SHOTS FOR 10 SECONDS" };
        Sprite[] prizeIcon = { ghost, mintS, gasS };
        for (int i = 0; i < 3; i++)
        {
            float y = 300 - i * 175;
            Icon(p, "Prize Icon " + i, prizeIcon[i], -255, y, 130);
            Transform pt2 = NewText(p, "Prize Text " + i);
            At(pt2, 85, y, 500, 150);
            var px2 = StyleTMP(pt2, 38, Ink, textMat);
            px2.text = prizeText[i];
            px2.enableWordWrapping = true;
            px2.alignment = TextAlignmentOptions.MidlineLeft;
        }
        Body(p, "Text", "THEY ARE RARE. BULLETS GO THROUGH THEM -\nTOUCH THEM WITH THE DOOGH!\nTHEY VANISH SOON AFTER THEY LAND.", -300, 200, 36);
        pages.Add(p.gameObject);

        // 5. Modes
        p = Page(card, "Page Modes", "GAME MODES");
        Row(p, "EASY", btnMedium, Hex("4CC864"), Ink, "SLOW AND RELAXED", 350);
        Row(p, "MEDIUM", btnMedium, Yellow, Ink, "NORMAL SPEED", 240);
        Row(p, "HARD", btnMedium, Hex("FF8C1E"), Ink, "FAST!", 130);
        Row(p, "TAHMA!!", btnMedium, Red, White, "CRAZY FAST!!", 20);
        Body(p, "Text", "THE LONGER YOU PLAY,\nTHE FASTER IT GETS.\n\nCHOOSE A MODE IN\nOPTIONS > GAME MODE\nEVERY MODE HAS ITS OWN TOP 10.", -220, 300, 36);
        pages.Add(p.gameObject);

        // 6. Menu buttons
        p = Page(card, "Page Menu", "BUTTONS");
        Row(p, "PLAY", btnMedium, Yellow, Ink, "TYPE YOUR NAME AND START", 330);
        Row(p, "OPTIONS", btnMedium, White, Ink, "SOUND VOLUME AND GAME MODE", 190);
        Row(p, "SCORES", btnMedium, White, Ink, "TOP 10 FOR EACH MODE", 50);
        Row(p, "QUIT", btnMedium, Ink, White, "CLOSE THE GAME", -90);
        Body(p, "Text", "MANY PLAYERS ON ONE PHONE?\nEACH ONE TYPES THEIR OWN NAME!", -250, 200, 40);
        pages.Add(p.gameObject);

        // Bottom: counter, SKIP, NEXT
        Transform counter = NewText(card, "Counter");
        Place(counter, -420, 300, 60);
        var ct = StyleTMP(counter, 40, new Color(0.45f, 0.45f, 0.5f), textMat);
        ct.text = "1 / 6";

        Transform mainPlay = Child(Child(canvas, "Main Menu"), "Play Button");
        Transform next = Object.Instantiate(mainPlay.gameObject, card).transform;
        next.name = "Next Button";
        StyleButton(next, "NEXT", btnLarge, Yellow, Ink, -530, 400, 120, 80, true);
        ((RectTransform)next).anchoredPosition = new Vector2(150, -530);

        Transform skip = Object.Instantiate(Child(Child(canvas, "OptionsMenu"), "Back Button").gameObject, card).transform;
        skip.name = "Skip Button";
        StyleButton(skip, "SKIP", btnSmall, Ink, White, -530, 220, 92, 50);
        ((RectTransform)skip).anchoredPosition = new Vector2(-230, -530);

        var view = go.AddComponent<TutorialView>();
        view.pages = pages.ToArray();
        view.counter = ct;
        view.nextLabel = Child(next, "Text") != null ? Child(next, "Text").GetComponent<TMP_Text>() : next.GetComponentInChildren<TMP_Text>();
        UnityEventTools.AddVoidPersistentListener(ClearClicks(next).onClick, view.Next);
        UnityEventTools.AddVoidPersistentListener(ClearClicks(skip).onClick, view.Skip);

        go.SetActive(true);
    }

    // ---------------------------------------------------------------- pause menu (Level_1)

    static void StylePauseMenu(Scene level)
    {
        GameObject cgo = level.GetRootGameObjects().FirstOrDefault(g => g.name.Trim() == "PauseMenu Canvas");
        if (cgo == null) { Debug.LogWarning("PauseMenu Canvas not found"); return; }
        var sc = cgo.GetComponent<CanvasScaler>();
        if (sc != null)
        {
            sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            sc.referenceResolution = new Vector2(900, 1600);
            sc.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            sc.matchWidthOrHeight = 0f;
        }
        var cv = cgo.GetComponent<Canvas>();
        if (cv != null) cv.sortingOrder = 15;

        Transform panel = Child(cgo.transform, "PauseMenu Panel");
        if (panel == null) return;
        Stretch(panel);
        var dim = Get<Image>(panel);
        dim.sprite = null;
        dim.color = new Color(0.04f, 0.03f, 0.08f, 0.65f);
        dim.raycastTarget = true;

        // ---- main pause card
        Transform main = Child(panel, "MainPauseMenu");
        if (main != null)
        {
            Stretch(main);
            Transform card = Picture(main, "Card", panelSprite, 0, 20, 700, 860, 0);
            card.GetComponent<Image>().preserveAspect = false;
            card.GetComponent<Image>().raycastTarget = true;

            Transform title = Child(main, "Title");
            if (title == null) title = NewText(main, "Title");
            Place(title, 390, 900, 220);
            title.localRotation = Quaternion.Euler(0, 0, 3);
            var tt = StyleTMP(title, 150, Yellow, titleMat);
            tt.text = "PAUSED";
            tt.characterSpacing = 4;

            StyleButton(Child(main, "Resume Button"), "RESUME", btnLarge, Yellow, Ink, 170, 540, 150, 96, true);
            StyleButton(Child(main, "Restart Button"), "RESTART", btnMedium, White, Ink, 0, 480, 110, 62);
            StyleButton(Child(main, "Option Button"), "OPTIONS", btnMedium, White, Ink, -140, 480, 110, 62);
            StyleButton(Child(main, "GoToMenu Button"), "MENU", btnSmall, Ink, White, -310, 360, 92, 54);
        }

        // ---- options card (volume)
        Transform opt = Child(panel, "OptionsMenu");
        if (opt != null)
        {
            Stretch(opt);
            Transform card = Picture(opt, "Card", panelSprite, 0, 20, 700, 860, 0);
            card.GetComponent<Image>().preserveAspect = false;
            card.GetComponent<Image>().raycastTarget = true;

            Transform ot = Child(opt, "Options Text");
            if (ot != null)
            {
                Place(ot, 390, 900, 220);
                ot.localRotation = Quaternion.Euler(0, 0, 3);
                var tx = StyleTMP(ot, 140, Yellow, titleMat);
                if (tx != null) { tx.text = "OPTIONS"; tx.characterSpacing = 4; }
            }

            Transform vt = Child(opt, "Volume Text");
            Place(vt, 170, 600, 80);
            var v = StyleTMP(vt, 66, Ink, textMat);
            if (v != null) v.text = "VOLUME";

            Transform slider = Child(opt, "Volume Slider");
            Place(slider, 70, 560, 46);
            Tint(Child(slider, "Background"), barSprite, Ink);
            Tint(Deep(slider, "Fill"), barSprite, Yellow);
            Tint(Deep(slider, "Handle"), checkSprite, White);
            var hr = (RectTransform)Deep(slider, "Handle");
            if (hr != null) hr.sizeDelta = new Vector2(70, 34);

            StyleButton(Child(opt, "Back Button"), "BACK", btnSmall, Ink, White, -310, 360, 92, 54);
        }
    }

    // ---------------------------------------------------------------- score badge (Level_1)

    // Replaces the plain yellow "0" with a comic badge: dark pill, coin, big yellow number.
    static void BuildScoreBadge(Scene level)
    {
        GameObject canvasGo = level.GetRootGameObjects().FirstOrDefault(g => g.name == "PauseButton Canvas");
        if (canvasGo == null) { Debug.LogWarning("PauseButton Canvas not found"); return; }

        // Turn off the old plain counter
        foreach (GameObject root in level.GetRootGameObjects())
        {
            if (root == canvasGo) continue;
            foreach (Coin_Counter cc in root.GetComponentsInChildren<Coin_Counter>(true))
            {
                cc.gameObject.SetActive(false);
                Object.DestroyImmediate(cc);
            }
        }

        var badgeGo = new GameObject("Score Badge", typeof(RectTransform), typeof(Image));
        badgeGo.layer = canvasGo.layer;
        Transform badge = badgeGo.transform;
        badge.SetParent(canvasGo.transform, false);
        var rt = (RectTransform)badge;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 1f);
        rt.anchoredPosition = new Vector2(0f, -30f);
        rt.sizeDelta = new Vector2(270f, 112f);
        var img = badgeGo.GetComponent<Image>();
        img.sprite = pillSprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.color = Ink;
        img.raycastTarget = false;

        Sprite coin = GameSprite("Assets/AmirGraphics/Coin3.png");
        if (coin != null)
        {
            Transform c = Picture(badge, "Coin", coin, -70f, 0f, 92f, 92f, 0);
            var crt = (RectTransform)c;
            crt.anchorMin = crt.anchorMax = new Vector2(0.5f, 0.5f);
            crt.anchoredPosition = new Vector2(-70f, 0f);
        }

        Transform num = NewText(badge, "Score");
        var nrt = (RectTransform)num;
        nrt.anchorMin = nrt.anchorMax = new Vector2(0.5f, 0.5f);
        nrt.pivot = new Vector2(0.5f, 0.5f);
        nrt.sizeDelta = new Vector2(170f, 110f);
        nrt.anchoredPosition = new Vector2(40f, 4f);
        var tx = StyleTMP(num, 108, Yellow, titleMat);
        tx.text = "0";

        var counter = badgeGo.AddComponent<Coin_Counter>();
        counter.label = tx;
        EditorUtility.SetDirty(counter);
    }

    // ---------------------------------------------------------------- pause button (Level_1)

    static void BuildPauseButton(Scene level)
    {
        foreach (GameObject g in level.GetRootGameObjects())
            if (g.name == "PauseButton Canvas") Object.DestroyImmediate(g);

        PauseMenu pm = null;
        foreach (GameObject g in level.GetRootGameObjects())
        {
            pm = g.GetComponentInChildren<PauseMenu>(true);
            if (pm != null) break;
        }
        if (pm == null) { Debug.LogWarning("PauseMenu not found"); return; }

        var canvasGo = new GameObject("PauseButton Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGo.layer = LayerMask.NameToLayer("UI");
        SceneManager.MoveGameObjectToScene(canvasGo, level);
        var c = canvasGo.GetComponent<Canvas>();
        c.renderMode = RenderMode.ScreenSpaceOverlay;
        c.sortingOrder = 5;
        var cs = canvasGo.GetComponent<CanvasScaler>();
        cs.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(900, 1600);
        cs.matchWidthOrHeight = 0f;

        var btnGo = new GameObject("Pause Button", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.layer = canvasGo.layer;
        btnGo.transform.SetParent(canvasGo.transform, false);
        var rt = (RectTransform)btnGo.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
        rt.pivot = new Vector2(1f, 1f);
        rt.anchoredPosition = new Vector2(-28f, -28f);
        rt.sizeDelta = new Vector2(120f, 120f);
        var img = btnGo.GetComponent<Image>();
        img.sprite = SpriteAt(UI + "PauseButton.png", Vector4.zero);
        img.preserveAspect = true;
        var b = btnGo.GetComponent<Button>();
        b.transition = Selectable.Transition.None;
        UnityEventTools.AddVoidPersistentListener(b.onClick, pm.Pause);
        if (btnGo.GetComponent<UIButtonFX>() == null) btnGo.AddComponent<UIButtonFX>();

        pm.pauseButton = btnGo;
        EditorUtility.SetDirty(pm);
    }

    // ---------------------------------------------------------------- game over (Level_1)

    static Transform NewButton(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        Transform t = NewText(go.transform, "Text");
        Stretch(t);
        return go.transform;
    }

    static Button ClearClicks(Transform t)
    {
        var b = t.GetComponent<Button>();
        for (int i = b.onClick.GetPersistentEventCount() - 1; i >= 0; i--)
            UnityEventTools.RemovePersistentListener(b.onClick, i);
        return b;
    }

    static void BuildGameOver(Scene level, LoserMenu loser)
    {
        GameObject old = level.GetRootGameObjects().FirstOrDefault(g => g.name == "GameOver Canvas");
        if (old != null) Object.DestroyImmediate(old);

        var cgo = new GameObject("GameOver Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        cgo.layer = 5;
        var cv = cgo.GetComponent<Canvas>();
        cv.renderMode = RenderMode.ScreenSpaceOverlay;
        cv.sortingOrder = 20;
        var sc = cgo.GetComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(900, 1600);
        sc.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        sc.matchWidthOrHeight = 0f;
        Transform canvas = cgo.transform;

        // Root panel (dim layer)
        var rootGo = new GameObject("GameOver Panel", typeof(RectTransform), typeof(Image));
        rootGo.layer = 5;
        Transform root = rootGo.transform;
        root.SetParent(canvas, false);
        Stretch(root);
        var dim = rootGo.GetComponent<Image>();
        dim.color = new Color(0.04f, 0.03f, 0.08f, 0.65f);
        Fade(root);

        Transform card = Picture(root, "Card", panelSprite, 0, 55, 760, 1080, 0);
        card.GetComponent<Image>().preserveAspect = false;
        card.GetComponent<Image>().raycastTarget = true;

        Transform title = NewText(root, "Title");
        Place(title, 520, 900, 220);
        title.localRotation = Quaternion.Euler(0, 0, 3);
        var tt = StyleTMP(title, 150, Yellow, titleMat);
        tt.text = "GAME OVER";
        tt.characterSpacing = 4;

        Transform nameT = NewText(root, "Name");
        Place(nameT, 390, 700, 90);
        var nt = StyleTMP(nameT, 62, Ink, textMat);
        nt.text = "PLAYER";
        nt.characterSpacing = 3;

        Transform scoreT = NewText(root, "Score");
        Place(scoreT, 270, 700, 200);
        var st = StyleTMP(scoreT, 190, Ink, textMat);
        st.text = "0";

        Transform badge = Picture(root, "Record Badge", pillSprite, 0, 150, 380, 80, 99);
        badge.GetComponent<Image>().preserveAspect = false;
        badge.GetComponent<Image>().color = new Color32(220, 30, 40, 255);
        badge.localRotation = Quaternion.Euler(0, 0, -4);
        Transform bt = NewText(badge, "Text");
        Stretch(bt);
        var btt = StyleTMP(bt, 50, White, textMat);
        btt.text = "NEW RECORD!";
        btt.characterSpacing = 3;

        Transform best = NewText(root, "Best");
        Place(best, 70, 720, 70);
        var bst = StyleTMP(best, 46, new Color32(90, 90, 100, 255), textMat);
        bst.text = "BEST";

        Transform restart = NewButton(root, "Restart Button");
        StyleButton(restart, "RESTART", btnLarge, Yellow, Ink, -80, 560, 150, 96, true);
        UnityEventTools.AddVoidPersistentListener(ClearClicks(restart).onClick, loser.Restart);

        Transform scoresBtn = NewButton(root, "Scores Button");
        StyleButton(scoresBtn, "SCORES", btnMedium, White, Ink, -240, 480, 110, 62);

        Transform change = NewButton(root, "ChangeName Button");
        StyleButton(change, "CHANGE NAME", btnMedium, White, Ink, -370, 480, 110, 62);
        UnityEventTools.AddVoidPersistentListener(ClearClicks(change).onClick, loser.ChangeName);

        Transform menu = NewButton(root, "Menu Button");
        StyleButton(menu, "MENU", btnSmall, Ink, White, -600, 360, 92, 54);
        UnityEventTools.AddVoidPersistentListener(ClearClicks(menu).onClick, loser.GoToMenu);

        // SCORES panel on top (same table as the main menu)
        var spGo = new GameObject("GameOver Scores", typeof(RectTransform), typeof(Image));
        spGo.layer = 5;
        Transform sp = spGo.transform;
        sp.SetParent(root, false);
        Stretch(sp);
        spGo.GetComponent<Image>().color = new Color(0.04f, 0.03f, 0.08f, 0.88f);
        NewText(sp, "LeaderBoardText");
        Transform spBack = NewButton(sp, "Back Button");
        BuildLeaderboard(sp);
        var spBackBtn = ClearClicks(spBack);
        UnityEventTools.AddBoolPersistentListener(spBackBtn.onClick, spGo.SetActive, false);
        UnityEventTools.AddBoolPersistentListener(ClearClicks(scoresBtn).onClick, spGo.SetActive, true);
        spGo.SetActive(false);

        var view = rootGo.AddComponent<GameOverView>();
        view.nameText = nt;
        view.scoreText = st;
        view.bestText = bst;
        view.recordBadge = badge.gameObject;
        view.scoresPanel = spGo;

        rootGo.SetActive(false);

        // The lose logic now opens this card instead of the old red text menu
        loser.pauseMenuUI = rootGo;
        loser.view = view;
        EditorUtility.SetDirty(loser);

        // Pause (Esc) must stay blocked while the GAME OVER card is open
        foreach (GameObject g in level.GetRootGameObjects())
        {
            var pm = g.GetComponentInChildren<PauseMenu>(true);
            if (pm != null) { pm.loserMenuUI = rootGo; EditorUtility.SetDirty(pm); }
        }
    }

    // ---------------------------------------------------------------- helpers

    static TMP_Text StyleTMP(Transform t, float size, Color color, Material mat)
    {
        var tx = t.GetComponent<TMP_Text>();
        if (tx == null) return null;
        tx.font = font;
        tx.fontSharedMaterial = mat;
        tx.fontSize = size;
        tx.enableAutoSizing = false;
        tx.fontStyle = FontStyles.Normal;
        tx.color = color;
        tx.enableVertexGradient = false;
        tx.alignment = TextAlignmentOptions.Center;
        tx.enableWordWrapping = false;
        tx.overflowMode = TextOverflowModes.Overflow;
        tx.raycastTarget = false;
        tx.characterSpacing = 0;
        tx.lineSpacing = 0;
        tx.margin = Vector4.zero;
        return tx;
    }

    static void StyleAny(Transform t, float size, Color color, string text, bool left = false)
    {
        if (t == null) return;
        var tx = StyleTMP(t, size, color, textMat);
        if (tx != null)
        {
            tx.text = text;
            if (left) tx.alignment = TextAlignmentOptions.MidlineLeft;
            return;
        }
        var lt = t.GetComponent<Text>();
        if (lt == null) return;
        lt.font = legacyFont;
        lt.fontSize = (int)size;
        lt.fontStyle = UnityEngine.FontStyle.Normal;
        lt.color = color;
        lt.text = text;
        lt.alignment = left ? TextAnchor.MiddleLeft : TextAnchor.MiddleCenter;
        lt.horizontalOverflow = HorizontalWrapMode.Overflow;
        lt.verticalOverflow = VerticalWrapMode.Overflow;
        lt.raycastTarget = false;
        var sh = t.GetComponent<Shadow>();
        if (sh != null) Object.DestroyImmediate(sh);
    }

    static Transform NewText(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.layer = parent.gameObject.layer;
        go.transform.SetParent(parent, false);
        return go.transform;
    }

    static void Tint(Transform t, Sprite sprite, Color color)
    {
        if (t == null) return;
        var img = t.GetComponent<Image>();
        if (img == null) return;
        img.sprite = sprite;
        img.type = Image.Type.Simple;
        img.preserveAspect = false;
        img.color = color;
    }

    static void Fade(Transform panel)
    {
        if (panel.GetComponent<CanvasGroup>() == null) panel.gameObject.AddComponent<CanvasGroup>();
        if (panel.GetComponent<UIPanelFade>() == null) panel.gameObject.AddComponent<UIPanelFade>();
    }

    static void Place(Transform t, float y, float w, float h)
    {
        if (t == null) return;
        var r = (RectTransform)t;
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.sizeDelta = new Vector2(w, h);
        r.anchoredPosition = new Vector2(0f, y);
        r.localScale = Vector3.one;
        r.localRotation = Quaternion.identity;
    }

    static void Stretch(Transform t)
    {
        if (t == null) return;
        var r = (RectTransform)t;
        r.anchorMin = Vector2.zero;
        r.anchorMax = Vector2.one;
        r.pivot = new Vector2(0.5f, 0.5f);
        r.offsetMin = Vector2.zero;
        r.offsetMax = Vector2.zero;
        r.localScale = Vector3.one;
        r.localRotation = Quaternion.identity;
    }

    static Transform Child(Transform parent, string name)
    {
        if (parent == null) return null;
        foreach (Transform c in parent)
            if (c.name.Trim() == name) return c;
        return null;
    }

    static Transform Deep(Transform parent, string name)
    {
        if (parent == null) return null;
        return parent.GetComponentsInChildren<Transform>(true).FirstOrDefault(c => c.name.Trim() == name);
    }

    static T Get<T>(Transform t) where T : Component
    {
        T c = t.GetComponent<T>();
        if (c == null) c = t.gameObject.AddComponent<T>();
        return c;
    }

    static Color Hex(string hex)
    {
        ColorUtility.TryParseHtmlString("#" + hex, out Color c);
        return c;
    }
}
