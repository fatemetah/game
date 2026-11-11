using System.Collections.Generic;
using System.Text;

// Makes Persian text display correctly in TextMeshPro:
// joins the letters (chooses the right letter shape) and puts them in right-to-left order.
// English letters and numbers inside the text keep their normal order.
public static class PersianText
{
    // isolated, final, initial, medial (0 = this shape does not exist)
    static readonly Dictionary<char, int[]> forms = new Dictionary<char, int[]>
    {
        { 'ء', new[] { 0xFE80, 0, 0, 0 } },
        { 'آ', new[] { 0xFE81, 0xFE82, 0, 0 } },
        { 'أ', new[] { 0xFE83, 0xFE84, 0, 0 } },
        { 'ؤ', new[] { 0xFE85, 0xFE86, 0, 0 } },
        { 'إ', new[] { 0xFE87, 0xFE88, 0, 0 } },
        { 'ئ', new[] { 0xFE89, 0xFE8A, 0xFE8B, 0xFE8C } },
        { 'ا', new[] { 0xFE8D, 0xFE8E, 0, 0 } },
        { 'ب', new[] { 0xFE8F, 0xFE90, 0xFE91, 0xFE92 } },
        { 'ة', new[] { 0xFE93, 0xFE94, 0, 0 } },
        { 'ت', new[] { 0xFE95, 0xFE96, 0xFE97, 0xFE98 } },
        { 'ث', new[] { 0xFE99, 0xFE9A, 0xFE9B, 0xFE9C } },
        { 'ج', new[] { 0xFE9D, 0xFE9E, 0xFE9F, 0xFEA0 } },
        { 'ح', new[] { 0xFEA1, 0xFEA2, 0xFEA3, 0xFEA4 } },
        { 'خ', new[] { 0xFEA5, 0xFEA6, 0xFEA7, 0xFEA8 } },
        { 'د', new[] { 0xFEA9, 0xFEAA, 0, 0 } },
        { 'ذ', new[] { 0xFEAB, 0xFEAC, 0, 0 } },
        { 'ر', new[] { 0xFEAD, 0xFEAE, 0, 0 } },
        { 'ز', new[] { 0xFEAF, 0xFEB0, 0, 0 } },
        { 'س', new[] { 0xFEB1, 0xFEB2, 0xFEB3, 0xFEB4 } },
        { 'ش', new[] { 0xFEB5, 0xFEB6, 0xFEB7, 0xFEB8 } },
        { 'ص', new[] { 0xFEB9, 0xFEBA, 0xFEBB, 0xFEBC } },
        { 'ض', new[] { 0xFEBD, 0xFEBE, 0xFEBF, 0xFEC0 } },
        { 'ط', new[] { 0xFEC1, 0xFEC2, 0xFEC3, 0xFEC4 } },
        { 'ظ', new[] { 0xFEC5, 0xFEC6, 0xFEC7, 0xFEC8 } },
        { 'ع', new[] { 0xFEC9, 0xFECA, 0xFECB, 0xFECC } },
        { 'غ', new[] { 0xFECD, 0xFECE, 0xFECF, 0xFED0 } },
        { 'ف', new[] { 0xFED1, 0xFED2, 0xFED3, 0xFED4 } },
        { 'ق', new[] { 0xFED5, 0xFED6, 0xFED7, 0xFED8 } },
        { 'ل', new[] { 0xFEDD, 0xFEDE, 0xFEDF, 0xFEE0 } },
        { 'م', new[] { 0xFEE1, 0xFEE2, 0xFEE3, 0xFEE4 } },
        { 'ن', new[] { 0xFEE5, 0xFEE6, 0xFEE7, 0xFEE8 } },
        { 'ه', new[] { 0xFEE9, 0xFEEA, 0xFEEB, 0xFEEC } },
        { 'و', new[] { 0xFEED, 0xFEEE, 0, 0 } },
        { 'پ', new[] { 0xFB56, 0xFB57, 0xFB58, 0xFB59 } }, // پ
        { 'چ', new[] { 0xFB7A, 0xFB7B, 0xFB7C, 0xFB7D } }, // چ
        { 'ژ', new[] { 0xFB8A, 0xFB8B, 0, 0 } },           // ژ
        { 'ک', new[] { 0xFB8E, 0xFB8F, 0xFB90, 0xFB91 } }, // ک
        { 'گ', new[] { 0xFB92, 0xFB93, 0xFB94, 0xFB95 } }, // گ
        { 'ی', new[] { 0xFBFC, 0xFBFD, 0xFBFE, 0xFBFF } }, // ی
    };

    const char ZWNJ = '‌'; // نیم‌فاصله

    public static bool IsPersianLetter(char c) { return forms.ContainsKey(c); }

    public static bool HasPersian(string s)
    {
        if (string.IsNullOrEmpty(s)) return false;
        foreach (char c in s) if (forms.ContainsKey(c)) return true;
        return false;
    }

    // Arabic keyboard letters -> Persian ones
    public static char Normalize(char c)
    {
        if (c == 'ي' || c == 'ى') return 'ی'; // ي ى -> ی
        if (c == 'ك') return 'ک';                   // ك -> ک
        return c;
    }

    static bool JoinsForward(char c)  // can connect to the next letter
    {
        int[] f;
        return forms.TryGetValue(c, out f) && f[2] != 0;
    }

    static bool IsAlef(char c) { return c == 'ا' || c == 'آ' || c == 'أ' || c == 'إ'; }

    static int LamAlef(char alef, bool joined)
    {
        switch (alef)
        {
            case 'آ': return joined ? 0xFEF6 : 0xFEF5;
            case 'أ': return joined ? 0xFEF8 : 0xFEF7;
            case 'إ': return joined ? 0xFEFA : 0xFEF9;
            default: return joined ? 0xFEFC : 0xFEFB;
        }
    }

    // Returns text ready to show in TextMeshPro. Text without Persian is returned unchanged.
    public static string Fix(string input)
    {
        if (!HasPersian(input)) return input;

        // 1) choose letter shapes (logical order)
        var shaped = new List<char>();
        var isRtl = new List<bool>();
        for (int i = 0; i < input.Length; i++)
        {
            char c = Normalize(input[i]);
            if (c == ZWNJ) continue;

            int[] f;
            if (!forms.TryGetValue(c, out f))
            {
                shaped.Add(c);
                isRtl.Add(false);
                continue;
            }

            char prev = i > 0 ? Normalize(input[i - 1]) : '\0';
            bool joinPrev = i > 0 && JoinsForward(prev);

            if (c == 'ل' && i + 1 < input.Length && IsAlef(input[i + 1]))
            {
                shaped.Add((char)LamAlef(input[i + 1], joinPrev));
                isRtl.Add(true);
                i++;
                continue;
            }

            char next = i + 1 < input.Length ? Normalize(input[i + 1]) : '\0';
            bool joinNext = f[2] != 0 && forms.ContainsKey(next);

            int code;
            if (joinPrev && joinNext) code = f[3];
            else if (joinPrev) code = f[1];
            else if (joinNext) code = f[2];
            else code = f[0];
            if (code == 0) code = f[0];

            shaped.Add((char)code);
            isRtl.Add(true);
        }

        // 2) visual order: right-to-left, but English/number runs stay left-to-right
        var runs = new List<KeyValuePair<bool, StringBuilder>>();
        for (int i = 0; i < shaped.Count; i++)
        {
            char c = shaped[i];
            bool rtl = isRtl[i];
            if (c == ' ')
            {
                // a space belongs to the run around it; between RTL letters it is RTL
                bool nextRtl = true;
                for (int k = i + 1; k < shaped.Count; k++)
                    if (shaped[k] != ' ') { nextRtl = isRtl[k]; break; }
                bool prevRtl = runs.Count == 0 || runs[runs.Count - 1].Key;
                rtl = prevRtl || nextRtl;
            }
            if (runs.Count == 0 || runs[runs.Count - 1].Key != rtl)
                runs.Add(new KeyValuePair<bool, StringBuilder>(rtl, new StringBuilder()));
            runs[runs.Count - 1].Value.Append(c);
        }

        var result = new StringBuilder();
        for (int r = runs.Count - 1; r >= 0; r--)
        {
            string s = runs[r].Value.ToString();
            if (runs[r].Key)
            {
                char[] arr = s.ToCharArray();
                System.Array.Reverse(arr);
                result.Append(arr);
            }
            else result.Append(s);
        }
        return result.ToString();
    }
}
