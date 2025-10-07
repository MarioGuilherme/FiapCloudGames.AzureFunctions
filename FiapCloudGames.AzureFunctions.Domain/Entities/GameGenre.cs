namespace FiapCloudGames.AzureFunctions.Domain.Entities;

public class GameGenre
{
    public int GameGenreId { get; private set; }
    public string Title { get; private set; } = null!;
    public virtual ICollection<Game> Games { get; } = [];
}
