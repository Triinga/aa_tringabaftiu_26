// Demonstrim i hashing-ut me chaining dhe linear probing.

var words = new[] { "A", "FOOL", "AND", "HIS", "MONEY", "ARE", "SOON", "PARTED" };

Console.WriteLine("=== Hash me chaining (open hashing) ===");
var chained = Hashing.ChainedTable(words, 13);
Console.WriteLine($"chainedTable(W, 13)[11] = {FormatList(chained[11])}");

Console.WriteLine("\n=== Linear probing (closed hashing) ===");
var table = new object?[13];
foreach (var word in words)
{
    var index = Hashing.ProbeInsert(table, word);
    Console.WriteLine($"Vendos {word,-7} në indeksin {index}");
}

Console.WriteLine($"probeFind(T, \"PARTED\") = {Hashing.ProbeFind(table, "PARTED")}");
Console.WriteLine($"remove(T, \"ARE\")      = {Hashing.Remove(table, "ARE")}");
Console.WriteLine($"probeFind(T, \"SOON\")   = {Hashing.ProbeFind(table, "SOON")}");
Console.WriteLine($"probeFind(T, \"ARE\")    = {Hashing.ProbeFind(table, "ARE")}");
Console.WriteLine($"remove(T, \"KID\")      = {Hashing.Remove(table, "KID")}");

Console.WriteLine("\nTabela pas heqjes së ARE:");
for (var i = 0; i < table.Length; i++)
    Console.WriteLine($"[{i,2}] {Hashing.DisplayCell(table[i])}");

Console.WriteLine("\n=== Double hashing (shembulli i ligjëratës) ===");
var numbers = new[] { 30, 20, 56, 75, 31, 19 };
var doubleHashTable = new object?[11];
foreach (var number in numbers)
    Hashing.DoubleHashInsert(doubleHashTable, number);

Console.WriteLine("D = [" + string.Join(", ", doubleHashTable.Select(Hashing.DisplayCell)) + "]");
Console.WriteLine($"doubleHashFind(D, 19) = {Hashing.DoubleHashFind(doubleHashTable, 19)}");
Console.WriteLine($"doubleHashFind(D, 42) = {Hashing.DoubleHashFind(doubleHashTable, 42)}");

Console.WriteLine("\n=== Krahasimet maksimale të kërkimit ===");
var maxN = Hashing.MaxComparisons(numbers, 11);
Console.WriteLine($"N: chained={maxN.Chained}, linear={maxN.Linear}, double={maxN.Double}");
var maxW = Hashing.MaxComparisons(words, 13);
Console.WriteLine($"W: chained={maxW.Chained}, linear={maxW.Linear}, double={maxW.Double}");

Console.WriteLine("\n=== Rritja automatike e tabelës ===");
object?[] growTable = new object?[5];
foreach (var word in words)
{
    growTable = Hashing.InsertGrow(growTable, word);
    Console.WriteLine($"Pas {word,-7}: madhësia {growTable.Length}");
}
Console.WriteLine($"G.Length = {growTable.Length}");
Console.WriteLine($"ProbeFind(G, \"ARE\") = {Hashing.ProbeFind(growTable, "ARE")}");

static string FormatList(List<string> values) => "[" + string.Join(", ", values) + "]";

static class Hashing
{
    public record ComparisonCounts(int Chained, int Linear, int Double);

    // Kjo shenjë e veçantë dallon qelizën e fshirë nga qeliza bosh (null).
    private static readonly object Deleted = new();

    public static int Num(object key)
    {
        if (key is int number)
            return number;

        if (key is not string word)
            throw new ArgumentException("Çelësi duhet të jetë numër i plotë ose tekst.");

        var sum = 0;
        foreach (var character in word)
        {
            var upper = char.ToUpperInvariant(character);
            if (upper < 'A' || upper > 'Z')
                throw new ArgumentException("Fjala duhet të përmbajë vetëm shkronja A-Z.");
            sum += upper - 'A' + 1;
        }

        return sum;
    }

    public static int H(object key, int capacity)
    {
        if (capacity <= 0)
            throw new ArgumentOutOfRangeException(nameof(capacity), "Madhësia duhet të jetë pozitive.");
        return ((Num(key) % capacity) + capacity) % capacity;
    }


    public static List<string>[] ChainedTable(IEnumerable<string> keys, int capacity)
    {
        var table = Enumerable.Range(0, capacity)
            .Select(_ => new List<string>())
            .ToArray();

        foreach (var key in keys)
            table[H(key, capacity)].Add(key);

        return table;
    }


    public static int ProbeInsert(object?[] table, object key)
    {
        var capacity = table.Length;
        var index = H(key, capacity);

        for (var checkedCells = 0; checkedCells < capacity; checkedCells++)
        {
            if (table[index] is null || ReferenceEquals(table[index], Deleted))
            {
                table[index] = key;
                return index;
            }
            index = (index + 1) % capacity;
        }

        throw new InvalidOperationException("Tabela është e mbushur; nuk ka qelizë të lirë.");
    }

    public static int ProbeFind(object?[] table, string key)
    {
        var capacity = table.Length;
        var index = H(key, capacity);

        for (var checkedCells = 0; checkedCells < capacity && table[index] is not null; checkedCells++)
        {
            if (!ReferenceEquals(table[index], Deleted) && Equals(table[index], key))
                return index;
            index = (index + 1) % capacity;
        }

        return -1;
    }

    public static int ProbeRemove(object?[] table, string key)
    {
        var capacity = table.Length;
        var index = H(key, capacity);

        for (var checkedCells = 0; checkedCells < capacity; checkedCells++)
        {
            if (table[index] is null)
                return -1;
            if (!ReferenceEquals(table[index], Deleted) && Equals(table[index], key))
            {
                table[index] = null;
                return index;
            }
            index = (index + 1) % capacity;
        }

        return -1;
    }

    // Heqja shënon qelizën, por nuk e bën null: kështu kërkimi kalon përtej saj.
    public static int Remove(object?[] table, string key)
    {
        var capacity = table.Length;
        var index = H(key, capacity);

        for (var checkedCells = 0; checkedCells < capacity; checkedCells++)
        {
            if (table[index] is null)
                return -1;
            if (!ReferenceEquals(table[index], Deleted) && Equals(table[index], key))
            {
                table[index] = Deleted;
                return index;
            }
            index = (index + 1) % capacity;
        }

        return -1;
    }

    // Double hashing: h jep fillimin, ndërsa hash-i i dytë jep madhësinë e hapit.
    public static int DoubleHashInsert(object?[] table, object key)
    {
        var capacity = table.Length;
        if (capacity <= 2)
            throw new ArgumentException("Madhësia e tabelës duhet të jetë numër prim më i madh se 2.");

        var index = H(key, capacity);
        var step = capacity - 2 - (Num(key) % (capacity - 2));

        for (var checkedCells = 0; checkedCells < capacity; checkedCells++)
        {
            if (table[index] is null || ReferenceEquals(table[index], Deleted))
            {
                table[index] = key;
                return index;
            }
            index = (index + step) % capacity;
        }

        throw new InvalidOperationException("Tabela është e mbushur; nuk ka qelizë të lirë.");
    }

    public static int DoubleHashFind(object?[] table, object key)
    {
        var capacity = table.Length;
        if (capacity <= 2)
            throw new ArgumentException("Madhësia e tabelës duhet të jetë numër prim më i madh se 2.");

        var index = H(key, capacity);
        var step = capacity - 2 - (Num(key) % (capacity - 2));

        for (var checkedCells = 0; checkedCells < capacity; checkedCells++)
        {
            if (table[index] is null)
                return -1;
            if (!ReferenceEquals(table[index], Deleted) && Equals(table[index], key))
                return index;
            index = (index + step) % capacity;
        }

        return -1;
    }

    // Për secilën metodë ndërton tabelën e vet dhe ruan numrin më të madh
    // të qelizave të kontrolluara në një kërkim të suksesshëm.
    public static ComparisonCounts MaxComparisons<T>(IReadOnlyList<T> keys, int capacity) where T : notnull
    {
        var chainedTable = Enumerable.Range(0, capacity)
            .Select(_ => new List<T>())
            .ToArray();
        var linearTable = new object?[capacity];
        var doubleTable = new object?[capacity];

        foreach (var key in keys)
        {
            chainedTable[H(key, capacity)].Add(key);
            ProbeInsert(linearTable, key);
            DoubleHashInsert(doubleTable, key);
        }

        var longestChain = chainedTable.Max(chain => chain.Count);
        var longestLinearSearch = 0;
        var longestDoubleSearch = 0;

        foreach (var key in keys)
        {
            longestLinearSearch = Math.Max(longestLinearSearch, CountProbeCells(linearTable, key));
            longestDoubleSearch = Math.Max(longestDoubleSearch, CountDoubleHashCells(doubleTable, key));
        }

        return new ComparisonCounts(longestChain, longestLinearSearch, longestDoubleSearch);
    }

    private static int CountProbeCells(object?[] table, object key)
    {
        var index = H(key, table.Length);
        for (var count = 1; count <= table.Length; count++)
        {
            if (Equals(table[index], key)) return count;
            index = (index + 1) % table.Length;
        }
        throw new InvalidOperationException("Çelësi nuk u gjet në tabelën linear probing.");
    }

    private static int CountDoubleHashCells(object?[] table, object key)
    {
        var capacity = table.Length;
        var index = H(key, capacity);
        var step = capacity - 2 - (Num(key) % (capacity - 2));
        for (var count = 1; count <= capacity; count++)
        {
            if (Equals(table[index], key)) return count;
            index = (index + step) % capacity;
        }
        throw new InvalidOperationException("Çelësi nuk u gjet në tabelën double hashing.");
    }

    // Fut çelësin dhe rrit tabelën vetëm kur faktori i ngarkesës kalon 0.5.
    public static object?[] InsertGrow(object?[] table, object key)
    {
        var keyCount = table.Count(cell => cell is not null && !ReferenceEquals(cell, Deleted));
        if ((keyCount + 1.0) / table.Length <= 0.5)
        {
            ProbeInsert(table, key);
            return table;
        }

        var newCapacity = NextPrime(2 * table.Length);
        var grownTable = new object?[newCapacity];

        // I lexojmë qelizat sipas indeksit dhe kopjojmë vetëm çelësat aktivë.
        foreach (var oldCell in table)
        {
            if (oldCell is not null && !ReferenceEquals(oldCell, Deleted))
                ProbeInsert(grownTable, oldCell);
        }

        ProbeInsert(grownTable, key);
        return grownTable;
    }

    private static int NextPrime(int minimumExclusive)
    {
        var candidate = minimumExclusive + 1;
        while (!IsPrime(candidate)) candidate++;
        return candidate;
    }

    private static bool IsPrime(int number)
    {
        if (number < 2) return false;
        for (var divisor = 2; divisor * divisor <= number; divisor++)
            if (number % divisor == 0) return false;
        return true;
    }

    public static string DisplayCell(object? cell)
    {
        if (cell is null) return "null";
        if (ReferenceEquals(cell, Deleted)) return "DELETED";
        return cell.ToString() ?? "null";
    }
}
