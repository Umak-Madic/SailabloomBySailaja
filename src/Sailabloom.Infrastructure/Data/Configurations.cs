using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sailabloom.Domain;

namespace Sailabloom.Infrastructure.Data;

public class PlantConfiguration : IEntityTypeConfiguration<Plant>
{
    public void Configure(EntityTypeBuilder<Plant> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(120);
        builder.Property(p => p.Category).IsRequired().HasMaxLength(60);
        builder.Property(p => p.Price).HasPrecision(18, 2);

        // Optimistic concurrency on stock: an UPDATE only succeeds if the
        // stock value read at claim time is still current. A competing
        // checkout that already decremented makes this save fail with a
        // concurrency exception instead of overselling.
        builder.Property(p => p.Stock).IsConcurrencyToken();

        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.ImageUrl).HasMaxLength(500);
    }
}

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.CustomerName).IsRequired().HasMaxLength(120);
        builder.Property(o => o.Email).IsRequired().HasMaxLength(256);
        builder.Property(o => o.Address).HasMaxLength(300);
        builder.Property(o => o.PaymentStatusText).HasMaxLength(160);
        builder.Property(o => o.Subtotal).HasPrecision(18, 2);
        builder.Property(o => o.ShippingCost).HasPrecision(18, 2);
        builder.Property(o => o.Total).HasPrecision(18, 2);
        builder.Property(o => o.CardBrand).HasMaxLength(40);
        builder.Property(o => o.CardLast4).HasMaxLength(4);

        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Refunds)
            .WithOne(r => r.Order)
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Replacements)
            .WithOne(r => r.Order)
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);
        builder.Property(i => i.PlantName).IsRequired().HasMaxLength(120);
        builder.Property(i => i.Category).HasMaxLength(60);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 2);
        builder.Property(i => i.LineTotal).HasPrecision(18, 2);
    }
}

public class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Amount).HasPrecision(18, 2);
        builder.Property(r => r.Note).HasMaxLength(500);
    }
}

public class ReplacementConfiguration : IEntityTypeConfiguration<Replacement>
{
    public void Configure(EntityTypeBuilder<Replacement> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.PlantName).IsRequired().HasMaxLength(120);
    }
}

public class IssueReportConfiguration : IEntityTypeConfiguration<IssueReport>
{
    public void Configure(EntityTypeBuilder<IssueReport> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ContactEmail).IsRequired().HasMaxLength(256);
        builder.Property(r => r.Description).IsRequired().HasMaxLength(2000);
        builder.Property(r => r.PhotoFileName).HasMaxLength(260);
        builder.Property(r => r.Status).HasMaxLength(40);

        builder.HasOne(r => r.Order)
            .WithMany()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
