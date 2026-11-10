using Microsoft.EntityFrameworkCore;
using Sailabloom.Application;
using Sailabloom.Application.Services;
using Sailabloom.Domain;
using Sailabloom.Infrastructure.Data;
using Sailabloom.Infrastructure.Repositories;
using Xunit;

namespace Sailabloom.Tests;

/// <summary>
/// The stock rules the owner asked to prove, against the EF Core InMemory
/// provider: carts never reserve stock, checkout claims it atomically,
/// refunds do not restock, and cancellations do.
/// </summary>
public class OrderServiceTests
{
    private static SailabloomDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<SailabloomDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        var context = new SailabloomDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }

    private static OrderService CreateService(SailabloomDbContext context)
    {
        var unitOfWork = new UnitOfWork(context);
        return new OrderService(
            new PlantRepository(context),
            new OrderRepository(context),
            new RefundRepository(context),
            new IssueReportRepository(context),
            unitOfWork);
    }

    private static async Task<Plant> AddPlantAsync(SailabloomDbContext context, int stock, decimal price = 10m)
    {
        var plant = new Plant
        {
            Name = "Test Fern",
            Category = "Fern",
            Price = price,
            Stock = stock,
            Description = "A plant used only by tests.",
            ImageUrl = string.Empty
        };

        context.Plants.Add(plant);
        await context.SaveChangesAsync();
        return plant;
    }

    private static PlaceOrderRequest Request(int plantId, int quantity) => new()
    {
        CustomerName = "Test Customer",
        Email = "test@example.com",
        Address = "1 Main Street",
        PaymentMethod = PaymentMethod.Card,
        Fulfillment = FulfillmentMethod.PrepaidLabel,
        CardBrand = "Visa",
        CardLast4 = "4242",
        Lines = new List<CartLineDto>
        {
            new() { PlantId = plantId, Quantity = quantity }
        }
    };

    [Fact]
    public async Task PlaceOrder_OverStock_IsRejectedAndStockUnchanged()
    {
        using var context = CreateContext();
        var service = CreateService(context);
        var plant = await AddPlantAsync(context, stock: 5);

        var result = await service.PlaceOrderAsync(Request(plant.Id, 6));

        Assert.False(result.Success);
        Assert.Equal(5, plant.Stock);
        Assert.Empty(await context.Orders.ToListAsync());
    }

    [Fact]
    public async Task PlaceOrder_Stock15_FirstComeFirstServed()
    {
        using var context = CreateContext();
        var service = CreateService(context);
        var plant = await AddPlantAsync(context, stock: 15);

        var first = await service.PlaceOrderAsync(Request(plant.Id, 10));
        Assert.True(first.Success);
        Assert.Equal(5, plant.Stock);

        // A second buyer asking for 10 against the remaining 5 is rejected whole.
        var second = await service.PlaceOrderAsync(Request(plant.Id, 10));
        Assert.False(second.Success);
        Assert.Equal(5, plant.Stock);

        var third = await service.PlaceOrderAsync(Request(plant.Id, 5));
        Assert.True(third.Success);
        Assert.Equal(0, plant.Stock);
    }

    [Fact]
    public async Task Refund_DoesNotRestock()
    {
        using var context = CreateContext();
        var service = CreateService(context);
        var plant = await AddPlantAsync(context, stock: 10);

        var placed = await service.PlaceOrderAsync(Request(plant.Id, 2));
        Assert.True(placed.Success);
        Assert.Equal(8, plant.Stock);

        var order = await service.GetOrderAsync(placed.OrderId!.Value);
        Assert.NotNull(order);

        var refund = await service.RefundAsync(
            order.Id, order.Total, RefundReason.DeadOrDamagedInTransit, "Arrived dead.", restock: false);

        Assert.True(refund.Success);
        Assert.Equal(8, plant.Stock);
    }

    [Fact]
    public async Task CancelOrder_RestocksItems()
    {
        using var context = CreateContext();
        var service = CreateService(context);
        var plant = await AddPlantAsync(context, stock: 10);

        var placed = await service.PlaceOrderAsync(Request(plant.Id, 3));
        Assert.True(placed.Success);
        Assert.Equal(7, plant.Stock);

        var cancel = await service.CancelOrderAsync(placed.OrderId!.Value);

        Assert.True(cancel.Success);
        Assert.Equal(10, plant.Stock);

        var order = await service.GetOrderAsync(placed.OrderId!.Value);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Cancelled, order.Status);
    }
}
