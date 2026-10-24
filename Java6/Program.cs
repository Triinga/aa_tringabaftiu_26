using System.Text;

long comparisons = 0;
long alignments = 0;

string traceText = "JIM SAM KE IN A BARBERSHOP";
string tracePattern = "BARBER";

Console.WriteLine($"Trace data: T = '{traceText}', P = '{tracePattern}'");
Console.WriteLine("Predicted/results:");
Console.WriteLine($"  shiftTable(P): {FormatShiftTable(ShiftTable(tracePattern))}");
ResetCounters();
Console.WriteLine($"  bruteForce: {BruteForce(traceText, tracePattern)}; counters {GetCounters()}");
ResetCounters();
Console.WriteLine($"  horspool:   {Horspool(traceText, tracePattern)}; counters {GetCounters()}");
ResetCounters();
Console.WriteLine($"  horspool2:  {Horspool2(traceText, tracePattern)}; counters {GetCounters()}");

Console.WriteLine();
Console.WriteLine("Task 2 examples:");
Console.WriteLine($"findAll(\"banana\", \"ana\"): [{string.Join(", ", FindAll("banana", "ana"))}]");
Console.WriteLine($"highlight(\"banana\", \"ana\"): {Highlight("banana", "ana")}");
Console.WriteLine($"goodSuffixTable(\"ABCBAB\"): [{string.Join(", ", GoodSuffixTable("ABCBAB"))}]");
ResetCounters();
Console.WriteLine($"boyerMoore: {BoyerMoore("BESS KNEW ABOUT BAOBAB", "BAOBAB")}; counters {GetCounters()}");

void ResetCounters()
{
    comparisons = 0;
    alignments = 0;
}

(long Comparisons, long Alignments) GetCounters() => (comparisons, alignments);

string FormatShiftTable(Dictionary<char, int> table) =>
    "{" + string.Join(", ", table.OrderBy(pair => pair.Key).Select(pair => $"'{pair.Key}': {pair.Value}")) + "}";

Dictionary<char, int> ShiftTable(string pattern)
{
    int m = pattern.Length;
    Dictionary<char, int> table = [];
    for (int j = 0; j < m - 1; j++)
        table[pattern[j]] = m - 1 - j;
    return table;
}

int Shift(Dictionary<char, int> table, char character, int patternLength) =>
    table.TryGetValue(character, out int value) ? value : patternLength;

int BruteForce(string text, string pattern)
{
    int n = text.Length;
    int m = pattern.Length;
    if (m == 0) return 0;

    for (int i = 0; i <= n - m; i++)
    {
        alignments++;
        int j = 0;
        while (j < m)
        {
            comparisons++;
            if (pattern[j] != text[i + j]) break;
            j++;
        }
        if (j == m) return i;
    }
    return -1;
}

int Horspool(string text, string pattern)
{
    int n = text.Length;
    int m = pattern.Length;
    if (m == 0) return 0;

    Dictionary<char, int> table = ShiftTable(pattern);
    int i = m - 1;
    while (i < n)
    {
        alignments++;
        int k = 0;
        while (k < m)
        {
            comparisons++;
            if (pattern[m - 1 - k] != text[i - k]) break;
            k++;
        }
        if (k == m) return i - m + 1;
        i += Shift(table, text[i], m);
    }
    return -1;
}

int Horspool2(string text, string pattern)
{
    int n = text.Length;
    int m = pattern.Length;
    if (m == 0) return 0;

    Dictionary<char, int> table = ShiftTable(pattern);
    int i = m - 1;
    while (i < n)
    {
        alignments++;
        int k = 0;
        while (k < m)
        {
            comparisons++;
            if (pattern[m - 1 - k] != text[i - k]) break;
            k++;
        }
        if (k == m) return i - m + 1;
        i += Shift(table, text[i - k], m);
    }
    return -1;
}

List<int> FindAll(string text, string pattern)
{
    List<int> matches = [];
    int n = text.Length;
    int m = pattern.Length;
    if (m == 0)
    {
        for (int i = 0; i <= n; i++) matches.Add(i);
        return matches;
    }

    Dictionary<char, int> table = ShiftTable(pattern);
    int iText = m - 1;
    while (iText < n)
    {
        alignments++;
        int k = 0;
        while (k < m)
        {
            comparisons++;
            if (pattern[m - 1 - k] != text[iText - k]) break;
            k++;
        }
        if (k == m) matches.Add(iText - m + 1);
        // Shifting by the rightmost-aligned text character preserves overlaps.
        iText += Shift(table, text[iText], m);
    }
    return matches;
}

string Highlight(string text, string pattern)
{
    List<int> matches = FindAll(text, pattern);
    if (matches.Count == 0) return text;

    StringBuilder result = new();
    int textPosition = 0;
    int matchIndex = 0;
    while (textPosition < text.Length)
    {
        if (matchIndex < matches.Count && textPosition == matches[matchIndex])
        {
            result.Append('[');
            for (int j = 0; j < pattern.Length; j++) result.Append(text[textPosition + j]);
            result.Append(']');
            textPosition += pattern.Length;
            matchIndex++;
        }
        else
        {
            result.Append(text[textPosition]);
            textPosition++;
        }
    }
    return result.ToString();
}

int[] GoodSuffixTable(string pattern)
{
    int m = pattern.Length;
    int[] d2 = Enumerable.Repeat(m, m).ToArray();
    if (m == 0) return d2;
    d2[0] = 0;

    for (int k = 1; k < m; k++)
    {
        bool found = false;
        for (int start = m - k - 1; start >= 0 && !found; start--)
        {
            bool equal = true;
            for (int j = 0; j < k; j++)
            {
                if (pattern[start + j] != pattern[m - k + j])
                {
                    equal = false;
                    break;
                }
            }
            if (!equal) continue;

            bool atPatternStart = start == 0;
            bool precedingCharactersDiffer = !atPatternStart && pattern[start - 1] != pattern[m - k - 1];
            if (atPatternStart || precedingCharactersDiffer)
            {
                d2[k] = m - k - start;
                found = true;
            }
        }

        if (!found)
        {
            int border = Math.Min(k - 1, m - k);
            while (border > 0)
            {
                bool equal = true;
                for (int j = 0; j < border; j++)
                {
                    if (pattern[j] != pattern[m - border + j])
                    {
                        equal = false;
                        break;
                    }
                }
                if (equal) break;
                border--;
            }
            d2[k] = m - border;
        }
    }
    return d2;
}

int BoyerMoore(string text, string pattern)
{
    int n = text.Length;
    int m = pattern.Length;
    if (m == 0) return 0;

    Dictionary<char, int> t1 = ShiftTable(pattern);
    int[] d2 = GoodSuffixTable(pattern);
    int s = 0;
    while (s <= n - m)
    {
        alignments++;
        int k = 0;
        while (k < m)
        {
            comparisons++;
            if (pattern[m - 1 - k] != text[s + m - 1 - k]) break;
            k++;
        }
        if (k == m) return s;

        char c = text[s + m - 1 - k];
        int badCharacterShift = Math.Max((t1.TryGetValue(c, out int value) ? value : m) - k, 1);
        int move = k == 0 ? badCharacterShift : Math.Max(badCharacterShift, d2[k]);
        s += move;
    }
    return -1;
}
