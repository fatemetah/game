using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class SkinAbility : MonoBehaviour
{
    readonly List<SkinProjectile> shots = new List<SkinProjectile>();
    string skin;
    Sprite projectileArt;
    Shooter shooter;
    SpriteRenderer player, helper;
    AudioSource music;
    bool musicPaused;

    void Awake()
    {
        skin = DooghSkins.Current;
        player = GetComponent<SpriteRenderer>();
        shooter = GetComponent<Shooter>();
        if (skin == "Water" || skin == "Juice" || skin == "Ayran")
            projectileArt = Resources.Load<Sprite>("SkinAbilities/" + skin);
        music = gameObject.AddComponent<AudioSource>();
        music.playOnAwake = false;
        music.loop = true;
        string track = skin == "Persian" ? "SkinAbilities/Traditional" : "Music/SugarHop";
        music.clip = Resources.Load<AudioClip>(track);
    }

    void OnEnable()
    {
        if (music != null)
        {
            SetMusicGain(OptionsMenu.MusicGain);
            OptionsMenu.MusicVolumeChanged += SetMusicGain;
            music.Play();
            musicPaused = false;
            SyncMusic();
        }
        if (skin == "Ayran")
            StartCoroutine(SpillFoam());
        else if (skin == "Water" || skin == "Milk")
            StartCoroutine(PerformAbility());
    }

    void Update()
    {
        if (music != null) SyncMusic();
    }

    void SyncMusic()
    {
        bool paused = Time.timeScale == 0f;
        if (paused == musicPaused) return;
        if (paused) music.Pause(); else music.UnPause();
        musicPaused = paused;
    }

    void SetMusicGain(float gain) { music.volume = 0.35f * gain; }

    void OnDisable()
    {
        StopAllCoroutines();
        OptionsMenu.MusicVolumeChanged -= SetMusicGain;
        if (music != null) music.Stop();
        if (helper != null) helper.gameObject.SetActive(false);
        foreach (var shot in shots)
            if (shot != null) shot.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        if (helper != null) Destroy(helper.gameObject);
        foreach (var shot in shots)
            if (shot != null) Destroy(shot.gameObject);
    }

    public bool RollOrangeSeed() => skin == "Juice" && Random.value < 0.20f;

    public void FireSeed(Vector3 origin, Vector2 direction)
    {
        Launch(origin, direction.normalized * 8.5f, 0f);
    }

    void Launch(Vector3 origin, Vector2 velocity, float gravity)
    {
        SkinProjectile available = null;
        foreach (var shot in shots)
            if (!shot.gameObject.activeSelf) { available = shot; break; }
        if (available == null)
        {
            available = SkinProjectile.Create(projectileArt, shooter.bulletPrefab, skin);
            shots.Add(available);
        }
        available.Launch(origin, velocity, gravity);
    }

    IEnumerator PerformAbility()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(7f, 12f));
            if (skin == "Milk") yield return MilkVisit();
            else yield return Waterfall();
        }
    }

    IEnumerator SpillFoam()
    {
        var delay = new WaitForSeconds(0.13f);
        while (true)
        {
            yield return delay;
            Bounds b = player.bounds;
            for (int side = -1; side <= 1; side += 2)
            {
                Vector3 rim = b.center + new Vector3(side * b.extents.x * 0.45f, b.extents.y * 0.60f);
                Launch(rim, new Vector2(side * Random.Range(1.4f, 2.6f), Random.Range(0.6f, 1.8f)), -7.5f);
            }
        }
    }

    IEnumerator Waterfall()
    {
        var delay = new WaitForSeconds(0.07f);
        for (int i = 0; i < 24; i++)
        {
            Bounds b = player.bounds;
            Vector3 mouth = new Vector3(b.center.x, b.max.y - 0.1f, b.center.z);
            Launch(mouth, new Vector2(1.9f * Mathf.Sin(i * 0.38f), 12f), -9f);
            yield return delay;
        }
    }

    IEnumerator MilkVisit()
    {
        Camera cam = Camera.main;
        if (cam == null) yield break;
        if (helper == null)
        {
            helper = new GameObject("Little milk").AddComponent<SpriteRenderer>();
            helper.sprite = player.sprite;
            helper.sortingLayerID = player.sortingLayerID;
            helper.sortingOrder = player.sortingOrder + 1;
            helper.transform.localScale = player.transform.lossyScale * 0.55f;
        }
        helper.gameObject.SetActive(true);
        float depth = cam.WorldToViewportPoint(player.bounds.center).z;
        Vector3 center = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, depth));
        Vector3 left = cam.ViewportToWorldPoint(new Vector3(-0.15f, 0.46f, depth));
        Vector3 right = cam.ViewportToWorldPoint(new Vector3(1.15f, 0.54f, depth));
        yield return Fly(left, center);
        const float patrolSeconds = 4.5f;
        const int shotsPerVisit = 9;
        int fired = 0;
        float started = Time.time;
        while (Time.time - started < patrolSeconds)
        {
            float progress = (Time.time - started) / patrolSeconds;
            float phase = progress * 2f * Mathf.PI;
            float sway = Mathf.Sin(phase);
            // A sideways figure eight keeps the helper moving even at the turns.
            helper.transform.position = cam.ViewportToWorldPoint(new Vector3(
                0.5f + 0.33f * sway, 0.5f + 0.17f * Mathf.Sin(2f * phase), depth));
            if (Time.timeScale > 0f && fired < shotsPerVisit && progress >= fired / (float)shotsPerVisit)
            {
                Vector3 mouth = helper.bounds.center + Vector3.up * helper.bounds.extents.y;
                shooter.ShootStandard(mouth, new Vector2(0.25f * sway, 1f));
                fired++;
            }
            yield return null;
        }
        yield return Fly(helper.transform.position, right);
        helper.gameObject.SetActive(false);
    }

    IEnumerator Fly(Vector3 from, Vector3 to)
    {
        for (float t = 0; t < 1f; t += Time.deltaTime / 0.55f)
        {
            helper.transform.position = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        helper.transform.position = to;
    }
}
