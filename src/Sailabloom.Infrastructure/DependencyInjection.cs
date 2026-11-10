using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sailabloom.Application.Repositories;
using Sailabloom.Application.Services;
using Sailabloom.Infrastructure.Data;
using Sailabloom.Infrastructure.Repositories;

namespace Sailabloom.Infrastructure;

/// <summary>
/// Registers the whole data + services stack. The database provider is
/// PLUG-AND-PLAY: it is chosen by the "DataProvider" configuration key
/// (InMemory | Sqlite | SqlServer) — switching databases is a config
/// change only, because all three EF Core providers are referenced.
/// The connection string comes from ConnectionStrings:Sailabloom.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddSailabloomInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["DataProvider"] ?? "InMemory";

        services.AddDbContext<SailabloomDbContext>(options =>
        {
            switch (provider.Trim().ToLowerInvariant())
            {
                case "sqlserver":
                    options.UseSqlServer(configuration.GetConnectionString("Sailabloom"));
                    break;

                case "sqlite":
                    options.UseSqlite(
                        configuration.GetConnectionString("Sailabloom") ?? "Data Source=sailabloom.db");
                    break;

                default:
                    options.UseInMemoryDatabase("SailabloomDb");
                    break;
            }
        });

        services.AddScoped<IPlantRepository, PlantRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IRefundRepository, RefundRepository>();
        services.AddScoped<IIssueReportRepository, IssueReportRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<ICatalogService, CatalogService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReportService, ReportService>();

        return services;
    }
}
