using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Spawner : MonoBehaviour
{
    [SerializeField] private float spawnenRate = 1f;

    [SerializeField] private GameObject[] enemyPrefabs;

    [SerializeField] private bool canSpawn = true;

    // The ghost is a prize, so it is rare:
    [Range(0f, 1f)] [SerializeField] private float ghostChance = 0.08f; // 8% of spawns at most
    [SerializeField] private float ghostCooldown = 25f;                  // at least 25 s between ghosts (all spawners)

    static float lastGhostTime = -999f;

    readonly List<GameObject> cans = new List<GameObject>();
    readonly List<GameObject> ghosts = new List<GameObject>();

    private void Start()
    {
        foreach (GameObject p in enemyPrefabs)
        {
            if (p == null) continue;
            if (p.CompareTag("ghost")) ghosts.Add(p); else cans.Add(p);
        }
        lastGhostTime = Time.time; // no ghost right at the start of a game
        StartCoroutine(Spawner());
    }

    private IEnumerator Spawner()
    {
        // Same random start as before, then scaled by the game mode and how long you've been playing
        float baseDelay = spawnenRate + Random.Range(0, 4);

        // The game should never start empty, but the cans shouldn't all drop at once either:
        // the first can comes after about 0.6 s, the next about 2 s later, then the next...
        float wait = 0.6f + StartOrder() * 2f + Random.Range(0f, 0.4f);

        while (canSpawn)
        {
            yield return new WaitForSeconds(wait);
            // after the first one: recalculated every time, so cans come more often as the game goes on
            wait = baseDelay * GameDifficulty.SpawnDelayMultiplier;
            if (PowerUps.MintActive) wait /= 0.55f;   // MINT: new cans also come slower
            GameObject e = Instantiate(Pick(), SpawnPoint(), Quaternion.identity);
            PushAboveScreen(e);
        }
    }

    // Which spawner goes first, second, third... (a different one each game)
    int StartOrder()
    {
        Enemy_Spawner[] all = FindObjectsOfType<Enemy_Spawner>();
        System.Array.Sort(all, (a, b) => a.transform.position.x.CompareTo(b.transform.position.x));
        int i = System.Array.IndexOf(all, this);
        int shift = (int)(Time.time * 10f) % Mathf.Max(1, all.Length);   // same for all spawners this frame
        return (i + shift) % Mathf.Max(1, all.Length);
    }

    GameObject Pick()
    {
        bool ghostReady = ghosts.Count > 0
            && Time.time - lastGhostTime >= ghostCooldown
            && GameObject.FindWithTag("ghost") == null
            && Random.value < ghostChance;

        if (ghostReady || cans.Count == 0)
        {
            lastGhostTime = Time.time;
            return ghosts[Random.Range(0, ghosts.Count)];
        }
        return cans[Random.Range(0, cans.Count)];
    }

    // Spawn above the top edge of the screen, so cans never pop up inside the play area
    // (on tall phones the camera shows more at the top than the spawners were placed for).
    static float ScreenTop()
    {
        Camera cam = Camera.main;
        if (cam == null || !cam.orthographic) return float.NegativeInfinity;
        return cam.transform.position.y + cam.orthographicSize;
    }

    Vector3 SpawnPoint()
    {
        Vector3 p = transform.position;
        p.y = Mathf.Max(p.y, ScreenTop() + 1.5f);
        return p;
    }

    static void PushAboveScreen(GameObject e)
    {
        float top = ScreenTop();
        if (float.IsNegativeInfinity(top)) return;
        Bounds b;
        var col = e.GetComponent<Collider2D>();
        var ren = e.GetComponentInChildren<Renderer>();
        if (ren != null) b = ren.bounds;
        else if (col != null) b = col.bounds;
        else return;
        float gap = (top + 0.2f) - b.min.y;
        if (gap > 0f) e.transform.position += new Vector3(0f, gap, 0f);
    }
}
