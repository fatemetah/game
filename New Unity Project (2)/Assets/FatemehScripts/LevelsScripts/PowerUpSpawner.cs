using UnityEngine;

// Now and then drops a MINT or a CO2 bubble from the top of the screen.
// Rare, like the ghost: at most one prize on screen, never while one is working.
public class PowerUpSpawner : MonoBehaviour
{
    public float cooldown = 20f;   // at least this many seconds between prizes
    public float chance = 0.07f;   // checked once per second after the cooldown

    float last;
    float nextCheck;
    float mintWeight, gasWeight;

    void Start()
    {
        last = Time.time;
        string skin = DooghSkins.Current;
        mintWeight = skin == "Mint" ? 2f : 1f;
        gasWeight = skin == "Gas" ? 2f : 1f;
    }

    void Update()
    {
        if (Time.time < nextCheck) return;
        nextCheck = Time.time + 1f;

        if (Time.time - last < cooldown) return;
        if (PowerUps.MintActive || PowerUps.GasActive) return;
        if (FindObjectOfType<PowerUpItem>() != null) return;
        if (GameObject.FindWithTag("ghost") != null) return;
        float totalWeight = mintWeight + gasWeight;
        if (Random.value >= Mathf.Clamp01(chance * totalWeight * 0.5f)) return;

        Camera cam = Camera.main;
        if (cam == null) return;
        float top = cam.transform.position.y + cam.orthographicSize;
        Vector3 pos = new Vector3(Random.Range(-2.8f, 2.8f), top + 1f, 0f);
        PowerUpItem.Create(Random.value * totalWeight < mintWeight ? PowerUpItem.Kind.Mint : PowerUpItem.Kind.Gas, pos);
        last = Time.time;
    }
}
