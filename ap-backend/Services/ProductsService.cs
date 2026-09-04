using AtelierPascaleWebsite.Data;
using AtelierPascaleWebsite.Models;
using AtelierPascaleWebsite.Models.DTOs;
using AtelierPascaleWebsite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtelierPascaleWebsite.Services
{
    public class ProductsService : IProductService
    {
        private readonly DatabaseContext _context;

        public ProductsService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<ProductResponseDTO?> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return null;
            }

            return new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                IsNewArrival = product.IsNewArrival,
                Images = product.Images.Select(i => new ProductImageDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };
        }

        public async Task<IEnumerable<ProductResponseDTO>> GetProductsByCategory(
            string categoryName,
            string? sortBy)
        {
            var formattedCategoryName = categoryName
                .Replace("-", " ")
                .Trim()
                .ToLower();

            var productsQuery = _context.Products
                .Include(p => p.Images)
                .AsQueryable();

            if (formattedCategoryName == "new arrival")
            {
                productsQuery = productsQuery.Where(p => p.IsNewArrival);
            }
            else
            {
                productsQuery = productsQuery.Where(
                    p => p.Category.Name.ToLower().Equals(formattedCategoryName));
            }

            var sortedQuery = sortBy switch
            {
                "price-low-to-high" => productsQuery.OrderBy(p => p.Price),
                "price-high-to-low" => productsQuery.OrderByDescending(p => p.Price),
                "name-a-to-z" => productsQuery.OrderBy(p => p.Name),
                "name-z-to-a" => productsQuery.OrderByDescending(p => p.Name),
                "newest" => productsQuery.OrderByDescending(p => p.CreatedAt),
                _ => productsQuery
            };

            productsQuery = sortedQuery;

            var products = await productsQuery.ToListAsync();
            return products.Select(product => new ProductResponseDTO
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                CategoryId = product.CategoryId,
                IsNewArrival = product.IsNewArrival,
                Images = product.Images.Select(i => new ProductImageDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ImageUrl = i.ImageUrl
                }).ToList()
            });
        }

        public async Task<ProductResponseDTO?> CreateProduct(
            ProductCreateRequestDTO productRequest)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == productRequest.CategoryId);

            if (!categoryExists)
            {
                throw new ArgumentException("Invalid category ID.");
            }

            var newProduct = new Product
            {
                Name = productRequest.Name,
                Description = productRequest.Description,
                Price = productRequest.Price,
                CategoryId = productRequest.CategoryId,
                IsNewArrival = productRequest.IsNewArrival
            };

            _context.Products.Add(newProduct);
            await _context.SaveChangesAsync();

            return new ProductResponseDTO
            {
                Id = newProduct.Id,
                Name = newProduct.Name,
                Description = newProduct.Description,
                Price = newProduct.Price,
                CategoryId = newProduct.CategoryId,
                IsNewArrival = newProduct.IsNewArrival,
                Images = newProduct.Images.Select(i => new ProductImageDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };
        }

        public async Task<ProductResponseDTO?> UpdateProduct(
            int id,
            ProductUpdateRequestDTO productRequest)
        {
            var existingProduct = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (existingProduct == null)
            {
                return null;
            }

            existingProduct.Name = productRequest.Name;
            existingProduct.Description = productRequest.Description;
            existingProduct.Price = productRequest.Price;
            existingProduct.CategoryId = productRequest.CategoryId;
            existingProduct.IsNewArrival = productRequest.IsNewArrival;

            var categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == productRequest.CategoryId);
                
            if (!categoryExists)
            {
                throw new ArgumentException("Invalid category ID.");
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductExists(id))
                {
                    return null;
                }

                throw;
            }

            return new ProductResponseDTO
            {
                Id = existingProduct.Id,
                Name = existingProduct.Name,
                Description = existingProduct.Description,
                Price = existingProduct.Price,
                CategoryId = existingProduct.CategoryId,
                IsNewArrival = existingProduct.IsNewArrival,
                Images = existingProduct.Images.Select(i => new ProductImageDTO
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ImageUrl = i.ImageUrl
                }).ToList()
            };
        }

        public async Task<bool> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return false;
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return true;
        }

        private Task<bool> ProductExists(int id)
        {
            return _context.Products.AnyAsync(p => p.Id == id);
        }
    }
}
