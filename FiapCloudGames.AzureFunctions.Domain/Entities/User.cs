namespace FiapCloudGames.AzureFunctions.Domain.Entities;

public class User
{
    public int UserId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Email { get; private set; } = null!;
}
