using AtelierPascaleWebsite.Models;
using AtelierPascaleWebsite.Models.DTOs;
using AtelierPascaleWebsite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using AtelierPascaleWebsite.Data;

namespace AtelierPascaleWebsite.Services
{
    public class ProductsService : IProductService
    {
        private readonly DatabaseContext _context;

        public ProductsService(DatabaseContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<ProductResponseDTO>> GetAllProducts(string? sortBy)
        {
            var products = await _context.Products.ToListAsync();
            return products.Select(p => new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId
            });
        }
        public async Task<ProductResponseDTO?> GetProductById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            return product == null ? null : new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId
            };
        }
        public async Task<IEnumerable<ProductResponseDTO>> GetProductsByCategory(string categoryName, string? sortBy)
        {
            var products = await _context.Products
                .Where(p => p.Category.Name == categoryName)
                .ToListAsync();
            return products.Select(p => new ProductResponseDTO
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                CategoryId = p.CategoryId
            });
        }
        public async Task<ProductResponseDTO?> CreateProduct(ProductCreateRequestDTO productRequest)
        {
            var newProduct = new Product
            {
                Name = productRequest.Name,
                Description = productRequest.Description,
                Price = productRequest.Price,
                CategoryId = productRequest.CategoryId
            };
            
            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();
            return new ProductResponseDTO
            {
                Id = newProduct.Id,
                Name = newProduct.Name,
                Description = newProduct.Description,
                Price = newProduct.Price,
                CategoryId = newProduct.CategoryId
            };
        }
        public async Task<ProductResponseDTO?> UpdateProduct(int id, ProductUpdateRequestDTO productRequest)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return null;
            existingProduct.Name = productRequest.Name;
            existingProduct.Description = productRequest.Description;
            existingProduct.Price = productRequest.Price;
            existingProduct.CategoryId = productRequest.CategoryId;
            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();
            return new ProductResponseDTO
            {
                Id = existingProduct.Id,
                Name = existingProduct.Name,
                Description = existingProduct.Description,
                Price = existingProduct.Price,
                CategoryId = existingProduct.CategoryId
            };
        }
        public async Task<bool> DeleteProduct(int id)
        {
            var existingProduct = await _context.Products.FindAsync(id);
            if (existingProduct == null) return false;
            _context.Products.Remove(existingProduct);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}