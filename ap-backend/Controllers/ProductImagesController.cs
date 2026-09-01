using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using AtelierPascaleWebsite.Models;
using AtelierPascaleWebsite.Models.DTOs;
using AtelierPascaleWebsite.Data;
using Microsoft.AspNetCore.RateLimiting;

[Route("api/[controller]")]
[ApiController]
public class ProductImagesController : ControllerBase
{
    private readonly DatabaseContext _context;
    public ProductImagesController(DatabaseContext context)
    {
        _context = context;
    }

    // GET: api/ProductImage
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProductImageDTO>>> GetProductImage()
    {
        return await _context.ProductImages
            .Select(pi => new ProductImageDTO
            {
                Id = pi.Id,
                ProductId = pi.ProductId,
                ImageUrl = pi.ImageUrl
            })
            .ToListAsync();
    }

    // GET: api/ProductImage/5
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductImageDTO>> GetProductImage(int id)
    {
        var productimage = await _context.ProductImages.FindAsync(id);

        if (productimage == null)
        {
            return NotFound();
        }

        return new ProductImageDTO
        {
            Id = productimage.Id,
            ProductId = productimage.ProductId,
            ImageUrl = productimage.ImageUrl
        };
    }

    // PUT: api/ProductImage/5
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpPut("{id}")]
    public async Task<IActionResult> PutProductImage(int? id, ProductImageDTO productimage)
    {
        if (id != productimage.Id)
        {
            return BadRequest();
        }

        var existingProductImage = await _context.ProductImages.FindAsync(id);
        if (existingProductImage == null) {
            return NotFound();
        }

        existingProductImage.ProductId = productimage.ProductId;
        existingProductImage.ImageUrl = productimage.ImageUrl;

        _context.Entry(existingProductImage).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ProductImageExists(id))
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

    // POST: api/ProductImage
    // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpPost]
    public async Task<ActionResult<ProductImageDTO>> PostProductImage(ProductImageDTO productimage)
    {

        var productExists = await _context.Products.AnyAsync(p => p.Id == productimage.ProductId);
        if (!productExists)
        {
            return NotFound();
        }

        var newProductImage = new ProductImage
        {
            ProductId = productimage.ProductId,
            ImageUrl = productimage.ImageUrl
        };

        _context.ProductImages.Add(newProductImage);
        await _context.SaveChangesAsync();

        return CreatedAtAction("GetProductImage", new { id = newProductImage.Id }, newProductImage);
    }

    // DELETE: api/ProductImage/5
    [Authorize(Roles = "Admin")]
    [EnableRateLimiting("Fixed")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProductImage(int? id)
    {
        var productimage = await _context.ProductImages.FindAsync(id);
        if (productimage == null)
        {
            return NotFound();
        }

        _context.ProductImages.Remove(productimage);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool ProductImageExists(int? id)
    {
        return _context.ProductImages.Any(e => e.Id == id);
    }
}


