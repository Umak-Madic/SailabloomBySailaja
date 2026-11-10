using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Sailabloom.Domain;

namespace Sailabloom.Infrastructure.Data;

/// <summary>
/// The single EF Core context for the store. It also carries the ASP.NET
/// Core Identity tables (IdentityDbContext) so the admin login and the
/// store data live in the same database, whichever provider is configured.
/// </summary>
public class SailabloomDbContext : IdentityDbContext<ApplicationUser>
{
    public SailabloomDbContext(DbContextOptions<SailabloomDbContext> options)
        : base(options)
    {
    }

    public DbSet<Plant> Plants => Set<Plant>();

    public DbSet<Order> Orders => Set<Order>();

    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    public DbSet<Refund> Refunds => Set<Refund>();

    public DbSet<Replacement> Replacements => Set<Replacement>();

    public DbSet<IssueReport> IssueReports => Set<IssueReport>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(SailabloomDbContext).Assembly);
    }
}
