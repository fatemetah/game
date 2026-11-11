using UnityEngine;

// The four game modes and what each one changes in the game.
// The chosen mode is saved on the device and used by the enemies and spawners.
public static class GameDifficulty
{
    public enum Level { Easy = 0, Medium = 1, Hard = 2, Tahma = 3 }

    const string PrefKey = "Difficulty";

    //                                 Easy   Medium  Hard   Tahma
    static readonly float[] speed      = { 0.75f, 1.0f,  1.35f, 1.8f }; // how fast the cans move
    static readonly float[] spawnDelay = { 1.6f,  1.0f,  0.8f,  0.6f }; // x time between new cans (smaller = more cans)

    public static Level Current
    {
        get { return (Level)Mathf.Clamp(PlayerPrefs.GetInt(PrefKey, (int)Level.Medium), 0, 3); }
        set
        {
            PlayerPrefs.SetInt(PrefKey, (int)value);
            PlayerPrefs.Save();
        }
    }

    // ---- Getting harder during a game ----
    // Each mode starts at its own values and slowly moves toward the NEXT mode only:
    // Easy -> Medium, Medium -> Hard, Hard -> Tahma, and Tahma gets a little more extreme.
    // Paused time does not count. After RampSeconds it stays there.
    public const float RampSeconds = 180f;   // 3 minutes to reach the next mode

    //                                  (after Tahma)
    const float TahmaMaxSpeed = 2.1f;
    const float TahmaMinDelay = 0.5f;

    // 0 at the start of a game -> 1 after RampSeconds
    public static float Progress { get { return Mathf.Clamp01(Time.timeSinceLevelLoad / RampSeconds); } }

    static float NextSpeed(int i) { return i < 3 ? speed[i + 1] : TahmaMaxSpeed; }
    static float NextDelay(int i) { return i < 3 ? spawnDelay[i + 1] : TahmaMinDelay; }

    public static float BaseSpeed { get { return speed[(int)Current]; } }
    public static float Speed
    {
        get { int i = (int)Current; return Mathf.Lerp(speed[i], NextSpeed(i), Progress); }
    }

    // Gravity grows with speed squared so the bounce arcs keep the same shape,
    // the cans just move through them faster.
    public static float Gravity { get { float s = Speed; return 0.5f * s * s; } }
    public static float SpawnDelayMultiplier
    {
        get { int i = (int)Current; return Mathf.Lerp(spawnDelay[i], NextDelay(i), Progress); }
    }
}
