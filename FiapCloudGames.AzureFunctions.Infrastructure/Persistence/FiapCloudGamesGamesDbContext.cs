using FiapCloudGames.AzureFunctions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FiapCloudGames.AzureFunctions.Infrastructure.Persistence;

public class FiapCloudGamesGamesDbContext(DbContextOptions<FiapCloudGamesGamesDbContext> options) : DbContext(options)
{
    public DbSet<Game> Games { get; set; }
    public DbSet<GameGenre> GameGenres { get; set; }
    public DbSet<Order> Orders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}
