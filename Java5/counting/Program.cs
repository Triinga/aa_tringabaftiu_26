

Record[] six =
[
    new("Arta", 9),
    new("Blend", 6),
    new("Dita", 9),
    new("Edon", 9),
    new("Fjolla", 6),
    new("Gent", 5)
];

Console.WriteLine("Input: " + string.Join(", ", six.Select(r => $"{r.Name}({r.Grade})")));
Print("c1", C1(six, r => r.Grade));
Print("c2", C2(six, r => r.Grade, 5, 10));
Print("c3", C3(six, r => r.Grade, 5, 10));

ResetCounters();
Record[] comparisonSorted = ComparisonCountingSort(six, r => r.Grade);
Print("ComparisonCountingSort", comparisonSorted);
Console.WriteLine($"Counters: {GetCounters()}");

ResetCounters();
Record[] distributionSorted = DistributionCountingSort(six, r => r.Grade, 5, 10);
Print("DistributionCountingSort", distributionSorted);
Console.WriteLine($"Counters after six: {GetCounters()}");

int[] numbers = [1003, 1001, 1002, 1001];
int[] sortedNumbers = DistributionCountingSort(numbers, x => x, 1001, 1003);
Console.WriteLine("Numbers: [" + string.Join(", ", sortedNumbers) + "]");

DateRecord[] recs =
[
    new(0, "2024-10-13"), new(1, "2024-01-20"), new(2, "2026-05-15"),
    new(3, "2024-01-15"), new(4, "2024-05-02"), new(5, "2026-08-27"),
    new(6, "2025-02-19"), new(7, "2026-11-22"), new(8, "2024-09-10"),
    new(9, "2026-12-11")
];

ResetCounters();
DateRecord[] datesSorted = SortByDate(recs);
Console.WriteLine("Date ids: " + string.Join(" ", datesSorted.Select(r => r.Id)));
Console.WriteLine($"Date sort counters: {GetCounters()}");

string[] words =
[
    "shi", "çaj", "llamba", "zemër", "dhe", "gur", "nënë", "tokë",
    "xhami", "emër", "rrugë", "cep", "ëndërr", "drita", "thikë",
    "lule", "njeri", "gjak", "zhurmë", "rosa", "sot", "xixa", "dhelpër"
];

Console.WriteLine("tokenize(llamba): [" + string.Join(", ", Tokenize("llamba")) + "]");
Console.WriteLine("tokenize(shtëpi): [" + string.Join(", ", Tokenize("shtëpi")) + "]");
ResetCounters();
string[] albanianSorted = SortAlbanian(words);
Console.WriteLine("Albanian words: " + string.Join(" ", albanianSorted));
Console.WriteLine($"Albanian sort counters: {GetCounters()}");

static void Print(string label, Record[] records) =>
    Console.WriteLine($"{label}: " + string.Join(", ", records.Select(r => r.Name)));

static Record[] C1(Record[] a, Func<Record, int> key)
{
    int n = a.Length;
    int[] count = new int[n];

    for (int i = 0; i < n - 1; i++)
        for (int j = i + 1; j < n; j++)
        {
            Counters.Comparisons++;
            if (key(a[i]) < key(a[j])) count[j]++;
            else count[i]++;
        }

    Record[] result = new Record[n];
    for (int i = 0; i < n; i++) result[count[i]] = a[i];
    return result;
}


static Record[] C2(Record[] a, Func<Record, int> key, int lo, int hi)
{
    int[] d = new int[hi - lo + 1];
    Counters.Passes++;
    foreach (Record record in a) d[key(record) - lo]++;
    Counters.Passes++;
    for (int j = 1; j < d.Length; j++) d[j] += d[j - 1];

    Record[] result = new Record[a.Length];
    Counters.Passes++;
    for (int i = a.Length - 1; i >= 0; i--)
    {
        int j = key(a[i]) - lo;
        result[d[j] - 1] = a[i];
        d[j]--;
    }
    return result;
}

static Record[] C3(Record[] a, Func<Record, int> key, int lo, int hi)
{
    int[] d = new int[hi - lo + 1];
    Counters.Passes++;
    foreach (Record record in a) d[key(record) - lo]++;
    Counters.Passes++;
    for (int j = 1; j < d.Length; j++) d[j] += d[j - 1];

    Record[] result = new Record[a.Length];
    Counters.Passes++;
    for (int i = 0; i < a.Length; i++)
    {
        int j = key(a[i]) - lo;
        result[d[j] - 1] = a[i];
        d[j]--;
    }
    return result;
}


static T[] DistributionCountingSort<T>(T[] a, Func<T, int> key, int lo, int hi)
{
    if (hi < lo)
        throw new ArgumentException("hi must be greater than or equal to lo.");

    Counters.Passes++;
    int[] d = new int[hi - lo + 1]; // hi is included, so add one

    foreach (T item in a)
    {
        int index = key(item) - lo;
        if (index < 0 || index >= d.Length)
            throw new ArgumentOutOfRangeException(nameof(key), "Every key must be in the range [lo, hi].");
        d[index]++;
    }

    for (int j = 1; j < d.Length; j++)
        d[j] += d[j - 1];

    T[] result = new T[a.Length];
    for (int i = a.Length - 1; i >= 0; i--)
    {
        int j = key(a[i]) - lo;
        result[d[j] - 1] = a[i];
        d[j]--;
    }

    return result;
}


static DateRecord[] SortByDate(DateRecord[] records)
{
    int minYear = int.MaxValue;
    int maxYear = int.MinValue;

    foreach (DateRecord record in records)
    {
        int year = int.Parse(record.Date.AsSpan(0, 4));
        if (year < minYear) minYear = year;
        if (year > maxYear) maxYear = year;
    }

    if (records.Length == 0)
    {
        minYear = 0;
        maxYear = 0;
    }

    DateRecord[] sorted = DistributionCountingSort(
        records, record => int.Parse(record.Date.AsSpan(8, 2)), 1, 31);
    sorted = DistributionCountingSort(
        sorted, record => int.Parse(record.Date.AsSpan(5, 2)), 1, 12);
    sorted = DistributionCountingSort(
        sorted, record => int.Parse(record.Date.AsSpan(0, 4)), minYear, maxYear);

    return sorted;
}


static int[] Tokenize(string word)
{
    string normalized = word.ToLowerInvariant();
    var values = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["a"] = 1,
        ["b"] = 2,
        ["c"] = 3,
        ["ç"] = 4,
        ["d"] = 5,
        ["dh"] = 6,
        ["e"] = 7,
        ["ë"] = 8,
        ["f"] = 9,
        ["g"] = 10,
        ["gj"] = 11,
        ["h"] = 12,
        ["i"] = 13,
        ["j"] = 14,
        ["k"] = 15,
        ["l"] = 16,
        ["ll"] = 17,
        ["m"] = 18,
        ["n"] = 19,
        ["nj"] = 20,
        ["o"] = 21,
        ["p"] = 22,
        ["q"] = 23,
        ["r"] = 24,
        ["rr"] = 25,
        ["s"] = 26,
        ["sh"] = 27,
        ["t"] = 28,
        ["th"] = 29,
        ["u"] = 30,
        ["v"] = 31,
        ["x"] = 32,
        ["xh"] = 33,
        ["y"] = 34,
        ["z"] = 35,
        ["zh"] = 36
    };

    string[] digraphs = ["dh", "gj", "ll", "nj", "rr", "sh", "th", "xh", "zh"];
    var tokens = new List<int>();

    for (int i = 0; i < normalized.Length;)
    {
        string? letter = null;
        if (i + 1 < normalized.Length)
        {
            string pair = normalized.Substring(i, 2);
            if (digraphs.Contains(pair, StringComparer.Ordinal))
                letter = pair;
        }

        letter ??= normalized.Substring(i, 1);
        if (!values.TryGetValue(letter, out int number))
            throw new ArgumentException($"Unsupported character or letter '{letter}' in word '{word}'.", nameof(word));

        tokens.Add(number);
        i += letter.Length;
    }

    return tokens.ToArray();
}


static string[] SortAlbanian(string[] words)
{
    TokenizedWord[] entries = words
        .Select(word => new TokenizedWord(word, Tokenize(word)))
        .ToArray();

    int longest = entries.Length == 0 ? 0 : entries.Max(entry => entry.Tokens.Length);
    TokenizedWord[] sorted = entries;

    for (int position = longest - 1; position >= 0; position--)
    {
        int currentPosition = position;
        sorted = DistributionCountingSort(
            sorted,
            entry => currentPosition < entry.Tokens.Length ? entry.Tokens[currentPosition] : 0,
            0,
            36);
    }

    return sorted.Select(entry => entry.Word).ToArray();
}


static Record[] ComparisonCountingSort(Record[] a, Func<Record, int> key)
{
    int n = a.Length;
    int[] count = new int[n];

    for (int i = 0; i < n - 1; i++)
    {
        for (int j = i + 1; j < n; j++)
        {
            Counters.Comparisons++;
            if (key(a[i]) <= key(a[j])) count[j]++;
            else count[i]++;
        }
    }

    Record[] result = new Record[n];
    for (int i = 0; i < n; i++) result[count[i]] = a[i];
    return result;
}

static void ResetCounters() => Counters.Reset();

static (int comparisons, int passes) GetCounters() =>
    (Counters.Comparisons, Counters.Passes);

record Record(string Name, int Grade);
record DateRecord(int Id, string Date);
record TokenizedWord(string Word, int[] Tokens);

static class Counters
{
    public static int Comparisons { get; set; }
    public static int Passes { get; set; }

    public static void Reset()
    {
        Comparisons = 0;
        Passes = 0;
    }
}
