using System.Security.Claims;
using AtelierPascaleWebsite.Models.DTOs;
using AtelierPascaleWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponseDTO>>> GetOrder()
    {
        var orders = await _orderService.GetOrders(
            GetCurrentUserId(),
            User.IsInRole("Admin"));

        return Ok(orders);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponseDTO>> GetOrder(int id)
    {
        try
        {
            var order = await _orderService.GetOrderById(
                id,
                GetCurrentUserId(),
                User.IsInRole("Admin"));

            if (order == null)
            {
                return NotFound();
            }

            return Ok(order);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [EnableRateLimiting("Fixed")]
    [HttpPost]
    public async Task<ActionResult<OrderResponseDTO>> PostOrder(
        OrderCreateRequestDTO order)
    {
        try
        {
            var confirmedOrder = await _orderService.CreateOrder(
                GetCurrentUserId(),
                order);

            return CreatedAtAction(
                nameof(GetOrder),
                new { id = confirmedOrder.OrderId },
                confirmedOrder);
        }
        catch (InvalidOperationException exception)
        {
            return BadRequest(exception.Message);
        }
        catch (Exception exception)
        {
            Console.WriteLine("ERROR CREATING ORDER:");
            Console.WriteLine(exception);

            return StatusCode(500, new
            {
                message = "Failed to create order",
                error = exception.Message
            });
        }
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
