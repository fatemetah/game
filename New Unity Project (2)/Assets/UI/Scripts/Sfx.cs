using System.Collections.Generic;
using UnityEngine;

// Plays the game's sound effects (files in Assets/Resources/Sfx).
// Usage: Sfx.Play("coin");
public static class Sfx
{
    // Several speakers: each sound gets its own, so changing the pitch of a new sound
    // (e.g. a gunshot) no longer bends a sound that is still playing (e.g. the ghost).
    const int Voices = 10;
    static AudioSource[] sources;
    static int next;
    static readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void ApplySavedVolume()
    {
        AudioListener.volume = OptionsMenu.SavedVolume01();
    }

    static AudioSource FreeSource()
    {
        if (sources == null)
        {
            var go = new GameObject("SFX");
            Object.DontDestroyOnLoad(go);
            sources = new AudioSource[Voices];
            for (int i = 0; i < Voices; i++)
            {
                var a = go.AddComponent<AudioSource>();
                a.playOnAwake = false;
                a.ignoreListenerPause = true;
                sources[i] = a;
            }
        }
        // prefer a speaker that is silent; otherwise reuse the oldest one
        for (int i = 0; i < Voices; i++)
        {
            int k = (next + i) % Voices;
            if (!sources[k].isPlaying) { next = (k + 1) % Voices; return sources[k]; }
        }
        AudioSource s = sources[next];
        next = (next + 1) % Voices;
        return s;
    }

    public static void Play(string name, float volume = 1f, float pitchVariation = 0.05f)
    {
        AudioClip clip;
        if (!clips.TryGetValue(name, out clip))
        {
            clip = Resources.Load<AudioClip>("Sfx/" + name);
            clips[name] = clip;
        }
        if (clip == null) return;
        AudioSource src = FreeSource();
        src.Stop();
        src.clip = clip;
        src.volume = volume;
        src.pitch = 1f + Random.Range(-pitchVariation, pitchVariation);
        src.Play();
    }
}
