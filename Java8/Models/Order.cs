using System.Globalization;

namespace FraudDetection.Models;

public record Order(string OrderId, string Email, string Address, DateTime Timestamp)
{
    public const string TimestampFormat = "yyyy-MM-dd HH:mm";

    public static Order Create(string orderId, string email, string address, string timestamp)
    {
        DateTime time = DateTime.ParseExact(timestamp, TimestampFormat, CultureInfo.InvariantCulture);
        return new Order(orderId, email, address, time);
    }
}
