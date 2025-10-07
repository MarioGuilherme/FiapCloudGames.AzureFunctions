using FiapCloudGames.AzureFunctions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FiapCloudGames.AzureFunctions.Infrastructure.Persistence;

public class FiapCloudGamesUsersDbContext(DbContextOptions<FiapCloudGamesUsersDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}
