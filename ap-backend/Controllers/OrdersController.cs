using AtelierPascaleWebsite.Data;
using AtelierPascaleWebsite.Models;
using AtelierPascaleWebsite.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using AtelierPascaleWebsite.Services;
using Microsoft.AspNetCore.RateLimiting;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly DatabaseContext _context;
    private readonly EmailSender _emailSender;
    public OrdersController(DatabaseContext context, EmailSender emailSender)
    {
        _context = context;
        _emailSender = emailSender;
    }

    // GET: api/Order
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderResponseDTO>>> GetOrder()
    {
        if (User.IsInRole("Admin"))
        {
            // If the user is an admin, return all orders
            var allOrders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Images)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return allOrders
                .Select(o => new OrderResponseDTO
                {
                    OrderId = o.Id,
                    Email = o.Email,
                    FirstName = o.FirstName,
                    LastName = o.LastName,
                    Status = o.Status,
                    TotalPrice = o.TotalPrice,
                    OrderDate = o.OrderDate,
                    OrderItems = o.OrderItems.Select(oi => new ItemsInOrderResponseDTO
                    {
                        ProductId = oi.ProductId,
                        OrderId = oi.OrderId,
                        Quantity = oi.Quantity,
                        PriceAtPurchase = oi.PriceAtPurchase,
                        ProductName = oi.Product!.Name,
                        ImageUrl = oi.Product!.Images.FirstOrDefault()?.ImageUrl ?? string.Empty
                    }).ToList()
                })
                .ToList();
        }

        // Get all orders for the current user
        var orders = await _context.Orders
            .Where(o => o.UserId == GetCurrentUserId())
            // Include the related OrderItems and their associated Products and Product Images
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Images)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders
            .Select(o => new OrderResponseDTO
            {
                OrderId = o.Id,
                Email = o.Email,
                FirstName = o.FirstName,
                LastName = o.LastName,
                Status = o.Status,
                TotalPrice = o.TotalPrice,
                OrderDate = o.OrderDate,
                OrderItems = o.OrderItems.Select(oi => new ItemsInOrderResponseDTO
                {
                    ProductId = oi.ProductId,
                    OrderId = oi.OrderId,
                    Quantity = oi.Quantity,
                    PriceAtPurchase = oi.PriceAtPurchase,
                    ProductName = oi.Product!.Name,
                    // Get the first image URL of the product, or an empty string if there are no images
                    ImageUrl = oi.Product!.Images.FirstOrDefault()?.ImageUrl ?? string.Empty
                }).ToList()
            })
            .ToList();
    }

    // GET: api/Order/5
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponseDTO>> GetOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);

        if (order == null)
        {
            return NotFound();
        }

        if (order.UserId != GetCurrentUserId() && !User.IsInRole("Admin"))
        {
            return Forbid();
        }

        return new OrderResponseDTO
        {
            OrderId = order.Id,
            Email = order.Email,
            FirstName = order.FirstName,
            LastName = order.LastName,
            Status = order.Status,
            TotalPrice = order.TotalPrice,
            OrderDate = order.OrderDate
        };
    }

    // POST: api/Order
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [EnableRateLimiting("Fixed")]
    [HttpPost]
    public async Task<ActionResult<OrderResponseDTO>> PostOrder(OrderCreateRequestDTO order)
    {
        try
        {
            // Get the user's shopping cart to calculate the total price of the order
            var shoppingCart = await _context.ShoppingCarts
            .Include(sc => sc.ItemsInCarts)
            .ThenInclude(iic => iic.Product)
            .FirstOrDefaultAsync(sc => sc.UserId == GetCurrentUserId());

            if (shoppingCart == null || shoppingCart.ItemsInCarts.Count == 0)
            {
                return BadRequest("Shopping cart is empty or does not exist.");
            }

            var totalPrice = shoppingCart.ItemsInCarts.Sum(iic => iic.Product!.Price * iic.Quantity);

            // Create a new order with the provided details and the calculated total price
            var confirmedOrder = new Order
            {
                UserId = GetCurrentUserId(),
                FirstName = order.FirstName,
                LastName = order.LastName,
                Email = order.Email,
                ShippingAddress = order.ShippingAddress,
                City = order.City,
                State = order.State,
                PostalCode = order.PostalCode,
                TotalPrice = totalPrice,
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.Orders.Add(confirmedOrder);
            await _context.SaveChangesAsync();

            // Add items from the shopping cart into the ItemsInOrder table directly after creating the order
            foreach (var item in shoppingCart.ItemsInCarts)
            {
                var itemInOrder = new ItemInOrder
                {
                    OrderId = confirmedOrder.Id,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.Product!.Price
                };
                _context.ItemsInOrders.Add(itemInOrder);
            }
            await _context.SaveChangesAsync();

            // Remove items from the shopping cart after creating the order
            _context.ItemsInCarts.RemoveRange(shoppingCart.ItemsInCarts);
            await _context.SaveChangesAsync();

            // Send a confirmation email to the user
            var receiverName = confirmedOrder.FirstName;
            var receiverEmail = confirmedOrder.Email;
            var subject = "Order Confirmation";
            var body = $@"
            <h2>Dear {confirmedOrder.FirstName},</h2>
            <p>Thank you for your order!</p>
            <p>Your order ID is <strong>{confirmedOrder.Id}</strong>.</p>
            <p>Total price: <strong>{confirmedOrder.TotalPrice:C}</strong></p>
            <p>We will notify you once your order is shipped.</p>
            <br />
            <p>Best regards,<br />Atelier Pascale</p>";

            await _emailSender.SendEmailAsync(receiverName, receiverEmail, subject, body);

            return CreatedAtAction("GetOrder", new { id = confirmedOrder.Id }, new OrderResponseDTO
            {
                OrderId = confirmedOrder.Id,
                OrderDate = confirmedOrder.OrderDate
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR CREATING ORDER:");
            Console.WriteLine(ex);

            return StatusCode(500, new
            {
                message = "Failed to create order",
                error = ex.Message
            });
        }
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
