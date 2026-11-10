using Microsoft.AspNetCore.Mvc;
using Sailabloom.Application;
using Sailabloom.Application.Services;

namespace Sailabloom.Api.Controllers;

/// <summary>Checkout + order lookup. All business rules live in OrderService.</summary>
[ApiController]
[Route("api/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orders;

    public OrdersController(IOrderService orders)
    {
        _orders = orders;
    }

    /// <summary>
    /// Place an order. Stock is claimed atomically; an over-stock cart is
    /// rejected as a whole with the reason in the result's Error field.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] PlaceOrderRequest request)
    {
        var result = await _orders.PlaceOrderAsync(request);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.OrderId }, result)
            : BadRequest(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var order = await _orders.GetOrderAsync(id);
        return order is null ? NotFound() : Ok(order);
    }
}
