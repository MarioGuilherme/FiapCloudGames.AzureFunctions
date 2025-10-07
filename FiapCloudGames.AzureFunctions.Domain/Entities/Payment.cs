namespace FiapCloudGames.AzureFunctions.Domain.Entities;

public class Payment(int userId, int orderId, decimal total, DateTime? paidAt = default, DateTime? canceledAt = default)
{
    public int PaymentId { get; private set; }
    public int UserId { get; private set; } = userId;
    public int OrderId { get; private set; } = orderId;
    public Guid? ExternalId { get; private set; }
    public decimal Total { get; private set; } = total;
    public DateTime? PaidAt { get; private set; } = paidAt;
    public DateTime? CanceledAt { get; private set; } = canceledAt;
}
