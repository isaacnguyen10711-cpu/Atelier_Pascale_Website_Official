using System.Security.Claims;
using AtelierPascaleWebsite.Models.DTOs;
using AtelierPascaleWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ItemsInCartController : ControllerBase
{
    private readonly IItemsInCartService _itemsInCartService;

    public ItemsInCartController(IItemsInCartService itemsInCartService)
    {
        _itemsInCartService = itemsInCartService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ItemsInCartResponseDTO>>> GetItemsInCart()
    {
        var items = await _itemsInCartService.GetItemsInCart(GetCurrentUserId());
        return Ok(items);
    }

    [EnableRateLimiting("Fixed")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutItemsInCart(
        int id,
        ItemsInCartUpdateRequestDTO item)
    {
        if (item.Quantity <= 0)
        {
            return BadRequest("Quantity must be at least 1.");
        }

        var updated = await _itemsInCartService.UpdateItemQuantity(
            id,
            GetCurrentUserId(),
            item.Quantity);

        return updated ? NoContent() : NotFound();
    }

    [EnableRateLimiting("Fixed")]
    [HttpPost]
    public async Task<ActionResult<ItemsInCartResponseDTO>> PostItemsInCart(
        ItemsInCartCreateRequestDTO item)
    {
        var createdItem = await _itemsInCartService.AddItemToCart(
            GetCurrentUserId(),
            item);

        if (createdItem == null)
        {
            return NotFound("Product or shopping cart not found.");
        }

        return Ok(createdItem);
    }

    [EnableRateLimiting("Fixed")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteItemsInCart(int id)
    {
        var deleted = await _itemsInCartService.DeleteItemFromCart(
            id,
            GetCurrentUserId());

        return deleted ? NoContent() : NotFound();
    }

    private int GetCurrentUserId()
    {
        return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }
}
