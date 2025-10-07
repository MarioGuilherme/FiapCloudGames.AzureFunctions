using FiapCloudGames.AzureFunctions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapCloudGames.AzureFunctions.Infrastructure.Persistence.Configurations;

public class GameGenreConfiguration : IEntityTypeConfiguration<GameGenre>
{
    public void Configure(EntityTypeBuilder<GameGenre> builder)
    {
        builder.HasKey(gr => gr.GameGenreId);

        builder.HasIndex(gr => gr.Title).IsUnique();
        builder.Property(gr => gr.Title)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasMany(gr => gr.Games)
               .WithMany(g => g.Genres);
    }
}
