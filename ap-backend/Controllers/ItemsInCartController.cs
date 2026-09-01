using AtelierPascaleWebsite.Data;
using AtelierPascaleWebsite.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using AtelierPascaleWebsite.Models.DTOs;
using Microsoft.AspNetCore.RateLimiting;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ItemsInCartController : ControllerBase
{
    private readonly DatabaseContext _context;
    public ItemsInCartController(DatabaseContext context)
    {
        _context = context;
    }

    // GET: api/ItemsInCart
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemsInCartResponseDTO>>> GetItemsInCart()
    {
        var itemsInCart = await _context.ItemsInCarts
            .Where(i => i.ShoppingCart != null && i.ShoppingCart.UserId == GetCurrentUserId())
            // Include the related Product and its Images
            .Select(p => new ItemsInCartResponseDTO
            {
                Id = p.Id,
                ProductId = p.Product!.Id,
                ProductName = p.Product!.Name,
                CategoryName = p.Product.Category.Name,
                Price = p.Product.Price,
                Quantity = p.Quantity,
                ProductImageUrl = p.Product.Images
                .Select(i => i.ImageUrl)
                .FirstOrDefault() ?? string.Empty,
            })
            .ToListAsync();

        return itemsInCart;
    }

    // PUT: api/ItemsInCart/5
    [EnableRateLimiting("Fixed")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutItemsInCart(int id, ItemsInCartUpdateRequestDTO itemsincart)
    {
        // Load the item in the shopping cart and check if it belongs to the current user
        var existingItem = await _context.ItemsInCarts
            .Include(i => i.ShoppingCart)
            .Where(i => i.ShoppingCart != null && i.ShoppingCart.UserId == GetCurrentUserId())
            .FirstOrDefaultAsync(i => i.Id == id);

        if (existingItem == null)
        {
            return NotFound();
        }

        if (itemsincart.Quantity <= 0)
        {
            return BadRequest("Quantity must be at least 1.");
        }

        existingItem.Quantity = itemsincart.Quantity;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // POST: api/ItemsInCart
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [EnableRateLimiting("Fixed")]
    [HttpPost]
    public async Task<ActionResult<ItemsInCartResponseDTO>> PostItemsInCart(ItemsInCartCreateRequestDTO itemsincart)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == itemsincart.ProductId);
        if (product == null)
        {
            return NotFound("Product not found.");
        }

        // Get the current user's shopping cart
        var shoppingCart = await _context.ShoppingCarts
            .Include(sc => sc.ItemsInCarts)
            .FirstOrDefaultAsync(sc => sc.UserId == GetCurrentUserId());

        if (shoppingCart == null)
        {
            return NotFound("Shopping cart not found.");
        }

        // Get the existing item in the shopping cart if there is one already
        var existingItem = shoppingCart.ItemsInCarts.FirstOrDefault(i => i.ProductId == itemsincart.ProductId);

        if (existingItem != null)
        {
            existingItem.Quantity += 1;
            await _context.SaveChangesAsync();
            return Ok(new ItemsInCartResponseDTO
            {
                Id = existingItem.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                CategoryName = product.Category.Name,
                Price = product.Price,
                Quantity = existingItem.Quantity,
                ProductImageUrl = product.Images
                    .Select(i => i.ImageUrl)
                    .FirstOrDefault() ?? string.Empty
            });
        }

        // If the item is not already in the shopping cart, add it to the user's  shopping cart
        var newItem = new ItemsInCart
        {
            ShoppingCartId = shoppingCart.Id,
            ProductId = itemsincart.ProductId,
            Quantity = 1
        };

        _context.ItemsInCarts.Add(newItem);
        await _context.SaveChangesAsync();

        return Ok(new ItemsInCartResponseDTO
        {
            Id = newItem.Id,
            ProductId = product.Id,
            ProductName = product.Name,
            CategoryName = product.Category.Name,
            Price = product.Price,
            Quantity = newItem.Quantity,
            ProductImageUrl = product.Images
                .Select(i => i.ImageUrl)
                .FirstOrDefault() ?? string.Empty
        });
    }

    // DELETE: api/ItemsInCart/5
    [EnableRateLimiting("Fixed")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItemsInCart(int? id)
    {
        // Load the item in the shopping cart and check if it belongs to the current user
        var itemsincart = await _context.ItemsInCarts
            .Include(i => i.ShoppingCart)
            .Where(i => i.ShoppingCart != null && i.ShoppingCart.UserId == GetCurrentUserId())
            .FirstOrDefaultAsync(i => i.Id == id);

        if (itemsincart == null)
        {
            return NotFound();
        }

        _context.ItemsInCarts.Remove(itemsincart);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}






