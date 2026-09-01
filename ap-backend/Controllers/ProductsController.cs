using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AtelierPascaleWebsite.Models;
using AtelierPascaleWebsite.Data;
using AtelierPascaleWebsite.Models.DTOs;
using Microsoft.AspNetCore.RateLimiting;

[Route("api/[controller]")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly DatabaseContext _context;
    public ProductsController(DatabaseContext context)
    {
        _context = context;
    }

    // GET: api/Product/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductResponseDTO>> GetProduct(int id)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .Where(p => p.Id == id)
            // Create a new ProductDTO object to return to prevent exposing the entity directly and to avoid circular references
            .Select(p => new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                IsNewArrival = p.IsNewArrival,
                Images = p.Images.Select(i => new ProductImageDTO
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl
                }).ToList()
            })
            .FirstOrDefaultAsync();

        if (product == null)
        {
            return NotFound();
        }

        return product;
    }

    // Filter products by category name
    [HttpGet("category/{categoryName}")] 
    public async Task<ActionResult<IEnumerable<ProductResponseDTO>>> GetProductsByCategory([FromRoute] string categoryName, [FromQuery] string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
        {
            return BadRequest("Category name cannot be empty.");
        }

        // Compare the category name by converting it to lowercase and replace hyphens with spaces
        var formattedCategoryName = categoryName.Replace("-", " ").Trim().ToLower();

        var productsQuery = _context.Products
            .Include(p => p.Images)
            .AsQueryable();

        if (formattedCategoryName == "new arrival")
        {
            productsQuery = productsQuery
                .Where(p => p.IsNewArrival);
        }
        else
        {
            productsQuery = productsQuery
                .Where(p => p.Category.Name.ToLower() == formattedCategoryName);
        }

        productsQuery = sortBy switch
        {
            "price-low-to-high" => productsQuery.OrderBy(p => p.Price),
            "price-high-to-low" => productsQuery.OrderByDescending(p => p.Price),
            "name-a-to-z" => productsQuery.OrderBy(p => p.Name),
            "name-z-to-a" => productsQuery.OrderByDescending(p => p.Name),
            "newest" => productsQuery.OrderByDescending(p => p.CreatedAt),
            _ => productsQuery
        };

        var products = await productsQuery
            .Select(p => new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId,
                IsNewArrival = p.IsNewArrival,
                Images = p.Images.Select(i => new ProductImageDTO
                {
                    Id = i.Id,
                    ImageUrl = i.ImageUrl
                }).ToList()
            })
            .ToListAsync();

        if (!products.Any())
        {
            return NotFound();
        }
        return products;
    }

    // PUT: api/Product/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProduct(int? id, ProductUpdateRequestDTO product)
    {
        if (id != product.Id)
        {
            return BadRequest();
        }

        var existingProduct = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

        if (existingProduct == null)
        {
            return NotFound();
        }

        // Update the properties of the existing product with the values from the DTO
        existingProduct.Name = product.Name;
        existingProduct.Description = product.Description;
        existingProduct.Price = product.Price;
        existingProduct.CategoryId = product.CategoryId;
        existingProduct.IsNewArrival = product.IsNewArrival;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // POST: api/Product
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpPost]
    public async Task<ActionResult<ProductResponseDTO>> PostProduct(ProductCreateRequestDTO product)
    {
        var categoryExists = await _context.Categories.AnyAsync(c => c.Id == product.CategoryId);
        if (!categoryExists)
        {
            return BadRequest("Invalid category ID.");
        }

        var newProduct = new Product
        {
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            CategoryId = product.CategoryId,
            IsNewArrival = product.IsNewArrival
        };

        _context.Products.Add(newProduct);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetProduct", new { id = newProduct.Id }, new ProductResponseDTO
        {
            Id = newProduct.Id,
            Name = newProduct.Name,
            Description = newProduct.Description,
            Price = newProduct.Price,
            CategoryId = newProduct.CategoryId,
            IsNewArrival = newProduct.IsNewArrival,
            Images = new List<ProductImageDTO>()
        });
    }

    // DELETE: api/Product/5
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int? id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound();
        }

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProductExists(int? id)
    {
        return _context.Products.Any(e => e.Id == id);
    }
}






