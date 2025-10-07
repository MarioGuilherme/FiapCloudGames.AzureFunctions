namespace FiapCloudGames.AzureFunctions.Domain.Entities;

public class Order(int userId)
{
    public int OrderId { get; private set; }
    public int? PaymentId { get; private set; }
    public int UserId { get; private set; } = userId;
    public DateTime OrderedAt { get; } = DateTime.Now;
    public DateTime? CanceledAt { get; private set; }
    public virtual ICollection<Game> Games { get; } = [];
}
