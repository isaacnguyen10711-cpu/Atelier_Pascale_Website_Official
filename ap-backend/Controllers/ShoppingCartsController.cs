using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AtelierPascaleWebsite.Models;
using AtelierPascaleWebsite.Data;
using System.Security.Claims;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ShoppingCartsController : ControllerBase
{
    private readonly DatabaseContext _context;
    public ShoppingCartsController(DatabaseContext context)
    {
        _context = context;
    }

    // GET: api/ShoppingCart
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ShoppingCart>>> GetShoppingCart()
    {
        return await _context.ShoppingCarts.ToListAsync();
    }

    // GET: api/ShoppingCart/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ShoppingCart>> GetShoppingCart(int id)
    {
        var shoppingcart = await _context.ShoppingCarts.FindAsync(id);

        if (shoppingcart == null)
        {
            return NotFound();
        }

        // Check if the user is an admin or the owner of the shopping cart
        if (!User.IsInRole("Admin") && shoppingcart.UserId != GetCurrentUserId())
        {
            return Forbid();
        }

        return shoppingcart;
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}


