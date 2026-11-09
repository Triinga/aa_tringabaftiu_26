using FraudDetection.Detection;
using FraudDetection.Models;
using FraudDetection.Normalization;
using FraudDetection.Tests;

if (args.Contains("--tests"))
{
    return EdgeCaseTests.RunAll();
}

var orders = new List<Order>
{
    Order.Create("ORD001", "john@email.com",    "123 Main Street, Apt 4B, NYC",          "2025-10-23 10:00"),
    Order.Create("ORD002", "jane@email.com",    "456 Oak Ave, Boston",                   "2025-10-23 10:15"),
    Order.Create("ORD003", "bob@email.com",     "123 Main St, Apartment 4B, NYC",        "2025-10-23 10:30"),
    Order.Create("ORD004", "alice@email.com",   "789 Pine Road, Seattle",                "2025-10-23 11:00"),
    Order.Create("ORD005", "charlie@email.com", "123 Main Street Apt 4-B New York City", "2025-10-23 12:00"),
};

var detector = new FraudDetector(new AddressNormalizer());
List<FraudAlert> alerts = detector.ProcessAll(orders);

Console.WriteLine("FRAUD ALERT:");
foreach (FraudAlert alert in alerts)
{
    Console.WriteLine(alert);
}

return 0;
