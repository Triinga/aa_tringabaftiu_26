using FraudDetection.Models;
using FraudDetection.Normalization;

namespace FraudDetection.Detection;

public class FraudDetector
{
    public static readonly TimeSpan Window = TimeSpan.FromHours(24);

    private readonly AddressNormalizer _normalizer;
    private readonly Dictionary<string, Queue<Order>> _recentOrdersByAddress = new();
    private DateTime? _lastTimestamp;

    public FraudDetector(AddressNormalizer normalizer)
    {
        _normalizer = normalizer;
    }

    public FraudAlert? Process(Order order)
    {
        if (_lastTimestamp.HasValue && order.Timestamp < _lastTimestamp.Value)
            throw new InvalidOperationException(
                $"Order {order.OrderId} arrived out of time order.");
        _lastTimestamp = order.Timestamp;

        string key = _normalizer.Normalize(order.Address);

        if (!_recentOrdersByAddress.TryGetValue(key, out Queue<Order>? recentOrders))
        {
            recentOrders = new Queue<Order>();
            _recentOrdersByAddress[key] = recentOrders;
        }

        while (recentOrders.Count > 0 && order.Timestamp - recentOrders.Peek().Timestamp > Window)
            recentOrders.Dequeue();

        FraudAlert? alert = null;
        foreach (Order previous in recentOrders)
        {
            if (!SameEmail(previous.Email, order.Email))
            {
                alert = new FraudAlert(order, previous);
                break;
            }
        }

        recentOrders.Enqueue(order);

        return alert;
    }

    public List<FraudAlert> ProcessAll(IEnumerable<Order> orders)
    {
        var alerts = new List<FraudAlert>();
        foreach (Order order in orders)
        {
            FraudAlert? alert = Process(order);
            if (alert != null)
                alerts.Add(alert);
        }
        return alerts;
    }

    private static bool SameEmail(string a, string b) =>
        string.Equals(a.Trim(), b.Trim(), StringComparison.OrdinalIgnoreCase);
}
