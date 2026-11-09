using FraudDetection.Detection;
using FraudDetection.Models;
using FraudDetection.Normalization;

namespace FraudDetection.Tests;

public static class EdgeCaseTests
{
    private static int _passed;
    private static int _failed;

    public static int RunAll()
    {
        Console.WriteLine("=== Normalization ===");
        NormalizesAllExampleVariantsToSameKey();
        HandlesCaseSpacesAndPunctuation();
        DifferentApartmentsStayDifferent();
        HandlesOtherVariationsOfTheSameKinds();

        Console.WriteLine("\n=== Fraud detection ===");
        ExactlyTwentyFourHoursIsFlagged();
        TwentyFourHoursAndOneMinuteIsNotFlagged();
        SameEmailIsNotFlagged();
        SameEmailDifferentCaseIsNotFlagged();
        EarliestMatchIsReported();
        EarliestMatchSkipsOrdersWithSameEmail();
        ExpiredOrderIsSkippedButNewerOneMatches();
        OutOfOrderTimestampIsRejected();

        Console.WriteLine($"\nResult: {_passed} passed, {_failed} failed");
        return _failed;
    }

    private static void NormalizesAllExampleVariantsToSameKey()
    {
        var n = new AddressNormalizer();
        string a = n.Normalize("123 Main Street, Apt 4B, NYC");
        string b = n.Normalize("123 Main St, Apartment 4B, NYC");
        string c = n.Normalize("123 Main Street Apt 4-B New York City");
        Check("example addresses -> '123 main st apt 4b nyc'",
            a == "123 main st apt 4b nyc" && a == b && b == c);
    }

    private static void HandlesCaseSpacesAndPunctuation()
    {
        var n = new AddressNormalizer();
        Check("case, extra spaces, dots, '#'",
            n.Normalize("  123   MAIN st.,  apt. #4-b ,   nyc ") == "123 main st apt 4b nyc");
    }

    private static void DifferentApartmentsStayDifferent()
    {
        var n = new AddressNormalizer();
        Check("apt 4B != apt 5B",
            n.Normalize("123 Main St, Apt 4B, NYC") != n.Normalize("123 Main St, Apt 5B, NYC"));
    }

    private static void HandlesOtherVariationsOfTheSameKinds()
    {
        var n = new AddressNormalizer();
        (string, string)[] pairs =
        {
            ("456 Oak Avenue, Suite 12, Boston", "456 OAK AVE. STE 12 BOSTON"),
            ("789 Pine Road, Apt 5 C, Seattle", "789 pine rd apt 5-c seattle"),
            ("10 Elm Street, Unit 2A, Chicago", "10 Elm St, Apt #2A, Chicago"),
            ("12 North Main Street, Los Angeles", "12 N Main St LA"),
            ("300 Fifth Avenue, Floor 3, NYC", "300 5th Ave, Fl 3, New York City"),
            ("1 Ocean Parkway, San Francisco", "1 ocean pkwy, sf"),
            ("123 East 45th Street, Apt 4 B", "123 E 45th St Apt 4B"),
        };

        foreach ((string a, string b) in pairs)
        {
            string na = n.Normalize(a);
            string nb = n.Normalize(b);
            Check($"'{a}' == '{b}'  ->  '{na}'", na == nb);
        }
    }

    private static void ExactlyTwentyFourHoursIsFlagged()
    {
        var alerts = Run(
            Order.Create("A", "a@x.com", "1 Elm Street", "2025-10-23 10:00"),
            Order.Create("B", "b@x.com", "1 Elm St",     "2025-10-24 10:00"));
        Check("exactly 24h later -> flagged", alerts.Count == 1 && alerts[0].MatchedOrder.OrderId == "A");
    }

    private static void TwentyFourHoursAndOneMinuteIsNotFlagged()
    {
        var alerts = Run(
            Order.Create("A", "a@x.com", "1 Elm Street", "2025-10-23 10:00"),
            Order.Create("B", "b@x.com", "1 Elm St",     "2025-10-24 10:01"));
        Check("24h + 1 min later -> not flagged", alerts.Count == 0);
    }

    private static void SameEmailIsNotFlagged()
    {
        var alerts = Run(
            Order.Create("A", "a@x.com", "1 Elm Street", "2025-10-23 10:00"),
            Order.Create("B", "a@x.com", "1 Elm St",     "2025-10-23 11:00"));
        Check("same email -> not flagged", alerts.Count == 0);
    }

    private static void SameEmailDifferentCaseIsNotFlagged()
    {
        var alerts = Run(
            Order.Create("A", "John@Email.com", "1 Elm Street", "2025-10-23 10:00"),
            Order.Create("B", "john@email.com", "1 Elm St",     "2025-10-23 11:00"));
        Check("same email, different letter case -> not flagged", alerts.Count == 0);
    }

    private static void EarliestMatchIsReported()
    {
        var alerts = Run(
            Order.Create("A", "a@x.com", "1 Elm Street", "2025-10-23 10:00"),
            Order.Create("B", "b@x.com", "1 Elm St",     "2025-10-23 11:00"),
            Order.Create("C", "c@x.com", "1 elm st.",    "2025-10-23 12:00"));
        Check("several matches -> earliest (A) reported for C",
            alerts.Count == 2 && alerts[1].SuspiciousOrder.OrderId == "C"
            && alerts[1].MatchedOrder.OrderId == "A");
    }

    private static void EarliestMatchSkipsOrdersWithSameEmail()
    {
        var alerts = Run(
            Order.Create("A", "c@x.com", "1 Elm Street", "2025-10-23 10:00"),
            Order.Create("B", "b@x.com", "1 Elm St",     "2025-10-23 11:00"),
            Order.Create("C", "c@x.com", "1 Elm St",     "2025-10-23 12:00"));
        Check("earliest match with a different email (B) reported for C",
            alerts.Count == 2 && alerts[1].SuspiciousOrder.OrderId == "C"
            && alerts[1].MatchedOrder.OrderId == "B");
    }

    private static void ExpiredOrderIsSkippedButNewerOneMatches()
    {
        var alerts = Run(
            Order.Create("A", "a@x.com", "1 Elm Street", "2025-10-23 08:00"),
            Order.Create("B", "b@x.com", "9 Oak Road",   "2025-10-23 09:00"),
            Order.Create("C", "c@x.com", "1 Elm St",     "2025-10-23 20:00"),
            Order.Create("D", "d@x.com", "1 Elm St",     "2025-10-24 09:00"));
        Check("expired order ignored, next one in window reported",
            alerts.Count == 2 && alerts[1].SuspiciousOrder.OrderId == "D"
            && alerts[1].MatchedOrder.OrderId == "C");
    }

    private static void OutOfOrderTimestampIsRejected()
    {
        var detector = new FraudDetector(new AddressNormalizer());
        detector.Process(Order.Create("A", "a@x.com", "1 Elm Street", "2025-10-23 10:00"));
        bool threw = false;
        try
        {
            detector.Process(Order.Create("B", "b@x.com", "1 Elm St", "2025-10-23 09:00"));
        }
        catch (InvalidOperationException)
        {
            threw = true;
        }
        Check("out-of-order timestamp -> rejected", threw);
    }

    private static List<FraudAlert> Run(params Order[] orders) =>
        new FraudDetector(new AddressNormalizer()).ProcessAll(orders);

    private static void Check(string name, bool condition)
    {
        if (condition) _passed++; else _failed++;
        Console.WriteLine($"  [{(condition ? "PASS" : "FAIL")}] {name}");
    }
}
