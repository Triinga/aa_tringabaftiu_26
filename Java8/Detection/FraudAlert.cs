using FraudDetection.Models;

namespace FraudDetection.Detection;

public record FraudAlert(Order SuspiciousOrder, Order MatchedOrder)
{
    public override string ToString() =>
        $"{SuspiciousOrder.OrderId} - Matches previous order {MatchedOrder.OrderId} (different email, same address)";
}
