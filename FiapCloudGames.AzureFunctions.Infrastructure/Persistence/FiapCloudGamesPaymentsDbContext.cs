using FiapCloudGames.AzureFunctions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace FiapCloudGames.AzureFunctions.Infrastructure.Persistence;

public class FiapCloudGamesPaymentsDbContext(DbContextOptions<FiapCloudGamesPaymentsDbContext> options) : DbContext(options)
{
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
}
