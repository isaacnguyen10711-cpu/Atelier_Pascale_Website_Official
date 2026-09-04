using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using AtelierPascaleWebsite.Models.DTOs;
using AtelierPascaleWebsite.Services.Interfaces;
using Microsoft.AspNetCore.RateLimiting;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    // GET: api/Product/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDTO>> GetProduct(int id)
    {
        var product = await _productService.GetProductById(id);

        if (product == null)
        {
            return NotFound();
        }

        return Ok(product);
    }

    // Filter products by category name
    [HttpGet("category/{categoryName}")] 
    public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetProductsByCategory([FromRoute] string categoryName, [FromQuery] string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return BadRequest("Category name cannot be empty.");
        }

        var products = (await _productService
            .GetProductsByCategory(categoryName, sortBy))
            .ToList();

        if (products.Count == 0)
        {
            return NotFound();
        }
        return Ok(products);
    }

    // PUT: api/Product/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int id, ProductUpdateRequestDTO product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        try
        {
            var updatedProduct = await _productService.UpdateProduct(id, product);
            if (updatedProduct == null)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    // POST: api/Product
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpPost]
    public async Task<ActionResult<ProductResponseDTO>> PostProduct(ProductCreateRequestDTO product)
    {
        try
        {
            var createdProduct = await _productService.CreateProduct(product);

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = createdProduct!.Id },
                createdProduct);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(exception.Message);
        }
    }

    // DELETE: api/Product/5
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var deleted = await _productService.DeleteProduct(id);
        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}






