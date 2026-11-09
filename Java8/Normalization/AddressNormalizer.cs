using System.Text;

namespace FraudDetection.Normalization;

public class AddressNormalizer
{
    private readonly Dictionary<string, string> _abbreviations = new();

    private static readonly HashSet<string> UnitTypes = new() { "apt", "ste", "fl" };

    private int _maxPhraseLength = 1;

    public AddressNormalizer()
    {
        AddStreetTypes();
        AddUnitTypes();
        AddDirections();
        AddOrdinals();
        AddCities();
    }

    private void AddStreetTypes()
    {
        AddAbbreviation("street", "st");
        AddAbbreviation("avenue", "ave");
        AddAbbreviation("av", "ave");
        AddAbbreviation("road", "rd");
        AddAbbreviation("boulevard", "blvd");
        AddAbbreviation("drive", "dr");
        AddAbbreviation("lane", "ln");
        AddAbbreviation("court", "ct");
        AddAbbreviation("place", "pl");
        AddAbbreviation("square", "sq");
        AddAbbreviation("highway", "hwy");
        AddAbbreviation("parkway", "pkwy");
        AddAbbreviation("circle", "cir");
        AddAbbreviation("terrace", "ter");
        AddAbbreviation("trail", "trl");
    }

    private void AddUnitTypes()
    {
        AddAbbreviation("apartment", "apt");
        AddAbbreviation("unit", "apt");
        AddAbbreviation("suite", "ste");
        AddAbbreviation("floor", "fl");
    }

    private void AddDirections()
    {
        AddAbbreviation("north", "n");
        AddAbbreviation("south", "s");
        AddAbbreviation("east", "e");
        AddAbbreviation("west", "w");
        AddAbbreviation("northeast", "ne");
        AddAbbreviation("northwest", "nw");
        AddAbbreviation("southeast", "se");
        AddAbbreviation("southwest", "sw");
    }

    private void AddOrdinals()
    {
        string[] words = { "first", "second", "third", "fourth", "fifth",
                           "sixth", "seventh", "eighth", "ninth", "tenth" };
        string[] numbers = { "1st", "2nd", "3rd", "4th", "5th",
                             "6th", "7th", "8th", "9th", "10th" };
        for (int i = 0; i < words.Length; i++)
            AddAbbreviation(words[i], numbers[i]);
    }

    private void AddCities()
    {
        AddAbbreviation("new york city", "nyc");
        AddAbbreviation("los angeles", "la");
        AddAbbreviation("san francisco", "sf");
        AddAbbreviation("washington dc", "dc");
    }

    public void AddAbbreviation(string fullForm, string shortForm)
    {
        string key = string.Join(' ', Tokenize(fullForm));
        _abbreviations[key] = shortForm;
        _maxPhraseLength = Math.Max(_maxPhraseLength, key.Split(' ').Length);
    }

    public string Normalize(string address)
    {
        ArgumentNullException.ThrowIfNull(address);

        string[] words = Tokenize(address);

        var result = new List<string>(words.Length);
        int i = 0;
        while (i < words.Length)
        {
            bool replaced = false;
            int longest = Math.Min(_maxPhraseLength, words.Length - i);

            for (int length = longest; length >= 1; length--)
            {
                string phrase = string.Join(' ', words, i, length);
                if (_abbreviations.TryGetValue(phrase, out string? shortForm))
                {
                    result.Add(shortForm);
                    i += length;
                    replaced = true;
                    break;
                }
            }

            if (!replaced)
            {
                result.Add(words[i]);
                i++;
            }
        }

        return string.Join(' ', JoinUnitLetters(result));
    }

    private static List<string> JoinUnitLetters(List<string> words)
    {
        var result = new List<string>(words.Count);
        for (int i = 0; i < words.Count; i++)
        {
            bool numberThenLetter = i > 0 && i + 1 < words.Count
                && UnitTypes.Contains(words[i - 1])
                && words[i].All(char.IsDigit)
                && words[i + 1].Length == 1
                && char.IsLetter(words[i + 1][0]);

            if (numberThenLetter)
            {
                result.Add(words[i] + words[i + 1]);
                i++;
            }
            else
            {
                result.Add(words[i]);
            }
        }
        return result;
    }

    private static string[] Tokenize(string text)
    {
        var builder = new StringBuilder(text.Length);

        foreach (char c in text.ToLowerInvariant())
        {
            if (char.IsLetterOrDigit(c))
                builder.Append(c);
            else if (c == '-' || c == '\'')
                continue;
            else
                builder.Append(' ');
        }

        return builder.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
}
