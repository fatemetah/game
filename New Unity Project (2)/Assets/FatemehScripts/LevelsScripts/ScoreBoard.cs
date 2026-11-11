using System;
using System.Collections.Generic;
using UnityEngine;
using System.Security.Cryptography;
using System.Text;

// Saves every player's best score on this phone, separately for each game mode.
public static class ScoreBoard
{
    [Serializable]
    public class Entry
    {
        public string name;
        public int score;
    }

    [Serializable]
    class Table
    {
        public List<Entry> entries = new List<Entry>();
    }

    const int MaxEntries = 50;

    static string Key(GameDifficulty.Level level) { return "Scores_" + (int)level; }

    // Each table is saved with a signature. If someone edits the saved scores by hand
    // (on a rooted phone, for example), the signature no longer matches and the edited table is ignored.
    const string Salt = "SugarRush-Doogh-7f3c91";

    static string Sign(string json)
    {
        using (var sha = SHA256.Create())
        {
            byte[] h = sha.ComputeHash(Encoding.UTF8.GetBytes(Salt + json + Salt));
            var sb = new StringBuilder();
            for (int i = 0; i < 12; i++) sb.Append(h[i].ToString("x2"));
            return sb.ToString();
        }
    }

    static Table Load(GameDifficulty.Level level)
    {
        string json = PlayerPrefs.GetString(Key(level), "");
        if (string.IsNullOrEmpty(json)) return new Table();

        string sig = PlayerPrefs.GetString(Key(level) + "_sig", "");
        if (sig == "")
        {
            // Scores saved by the older version (no signature yet): keep them and sign them now
            PlayerPrefs.SetString(Key(level) + "_sig", Sign(json));
            PlayerPrefs.Save();
        }
        else if (sig != Sign(json))
        {
            Debug.LogWarning("Saved scores were changed by hand - ignoring them.");
            return new Table();
        }

        Table t = JsonUtility.FromJson<Table>(json);
        return t ?? new Table();
    }

    static void Save(GameDifficulty.Level level, Table t)
    {
        string json = JsonUtility.ToJson(t);
        PlayerPrefs.SetString(Key(level), json);
        PlayerPrefs.SetString(Key(level) + "_sig", Sign(json));
        PlayerPrefs.Save();
    }

    // Records a finished game. Only the player's best score is kept.
    // Returns true if this score is now the #1 score for that mode.
    public static bool Submit(string name, GameDifficulty.Level level, int score)
    {
        if (score <= 0) return false;
        if (string.IsNullOrEmpty(name)) name = "PLAYER";

        Table t = Load(level);
        Entry mine = t.entries.Find(e => e.name == name);
        if (mine == null) t.entries.Add(new Entry { name = name, score = score });
        else if (score > mine.score) mine.score = score;
        else return false;

        t.entries.Sort((a, b) => b.score.CompareTo(a.score));
        if (t.entries.Count > MaxEntries) t.entries.RemoveRange(MaxEntries, t.entries.Count - MaxEntries);
        Save(level, t);
        return t.entries[0].name == name && t.entries[0].score == score;
    }

    public static List<Entry> Top(GameDifficulty.Level level, int count)
    {
        Table t = Load(level);
        t.entries.Sort((a, b) => b.score.CompareTo(a.score));
        return t.entries.GetRange(0, Mathf.Min(count, t.entries.Count));
    }
}
