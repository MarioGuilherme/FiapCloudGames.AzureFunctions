namespace FiapCloudGames.AzureFunctions.Domain.Entities;

public class Game
{
    public int GameId { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public virtual ICollection<GameGenre> Genres { get; private set; } = [];
    public virtual ICollection<Order> Orders { get; private set; } = [];
}
