using AtelierPascaleWebsite.Models.DTOs;
using AtelierPascaleWebsite.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

[Route("api/[controller]")]
[ApiController]
public class ProductImagesController : ControllerBase
{
    private readonly IProductImageService _productImageService;

    public ProductImagesController(IProductImageService productImageService)
    {
        _productImageService = productImageService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductImageDTO>>> GetProductImage()
    {
        var productImages = await _productImageService.GetAllProductImages();
        return Ok(productImages);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductImageDTO>> GetProductImage(int id)
    {
        var productImage = await _productImageService.GetProductImageById(id);

        if (productImage == null)
        {
            return NotFound();
        }

        return Ok(productImage);
    }

    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProductImage(int id, ProductImageDTO productImage)
    {
        if (id != productImage.Id)
        {
            return BadRequest();
        }

        var updatedProductImage = await _productImageService
            .UpdateProductImage(id, productImage);

        if (updatedProductImage == null)
        {
            return NotFound();
        }

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpPost]
    public async Task<ActionResult<ProductImageDTO>> PostProductImage(
        ProductImageDTO productImage)
    {
        var createdProductImage = await _productImageService
            .CreateProductImage(productImage);

        if (createdProductImage == null)
        {
            return NotFound();
        }

        return CreatedAtAction(
            nameof(GetProductImage),
            new { id = createdProductImage.Id },
            createdProductImage);
    }

    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductImage(int id)
    {
        var deleted = await _productImageService.DeleteProductImage(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}
