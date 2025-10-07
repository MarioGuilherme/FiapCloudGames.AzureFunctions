using FiapCloudGames.AzureFunctions.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FiapCloudGames.AzureFunctions.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.PaymentId);

        builder.Property(p => p.UserId).IsRequired();

        builder.Property(p => p.PaymentId).IsRequired();

        builder.Property(p => p.OrderId).IsRequired();

        builder.Property(p => p.ExternalId).IsRequired(false);

        builder.Property(p => p.Total).HasPrecision(18, 2);
    }
}
