using UnityEngine;
using UnityEngine.SceneManagement;

// The two prizes besides the ghost:
//   MINT  - icy: everything (cans, sugar, new arrivals) slows down for 10 seconds.
//   CO2   - fizz: the doogh shoots in 3 directions and much faster for 10 seconds.
// Timers use game time, so they stop while the game is paused.
public static class PowerUps
{
    public const float Duration = 10f;
    public const float SlowFactor = 0.4f;   // mint: enemies move at 40% speed

    static float mintEnd = -1f, gasEnd = -1f;

    public static bool MintActive { get { return Time.time < mintEnd; } }
    public static bool GasActive { get { return Time.time < gasEnd; } }
    public static float MintLeft { get { return Mathf.Max(0f, mintEnd - Time.time); } }
    public static float GasLeft { get { return Mathf.Max(0f, gasEnd - Time.time); } }

    public static void Mint()
    {
        mintEnd = Time.time + Duration;
        Sfx.Play("mint", 0.9f, 0f);
        PowerUpHud.Ensure();
    }

    public static void Gas()
    {
        gasEnd = Time.time + Duration;
        Sfx.Play("gas", 0.9f, 0f);
        PowerUpHud.Ensure();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init()
    {
        SceneManager.sceneLoaded += (scene, mode) =>
        {
            mintEnd = gasEnd = -1f;                     // a new game starts without prizes
            if (scene.buildIndex == 1)                   // the game scene
                new GameObject("PowerUp Spawner").AddComponent<PowerUpSpawner>();
        };
    }
}
