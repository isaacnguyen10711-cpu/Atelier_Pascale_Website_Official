using AtelierPascaleWebsite.Data;
using AtelierPascaleWebsite.Models;
using AtelierPascaleWebsite.Models.DTOs;
using AtelierPascaleWebsite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtelierPascaleWebsite.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly DatabaseContext _context;

        public ProductImageService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductImageDTO>> GetAllProductImages()
        {
            return await _context.ProductImages
                .Select(image => new ProductImageDTO
                {
                    Id = image.Id,
                    ProductId = image.ProductId,
                    ImageUrl = image.ImageUrl
                })
                .ToListAsync();
        }

        public async Task<ProductImageDTO?> GetProductImageById(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);
            return productImage == null ? null : ToResponseDTO(productImage);
        }

        public async Task<ProductImageDTO?> CreateProductImage(ProductImageDTO productImage)
        {
            var productExists = await _context.Products
                .AnyAsync(product => product.Id == productImage.ProductId);

            if (!productExists)
            {
                return null;
            }

            var newProductImage = new ProductImage
            {
                ProductId = productImage.ProductId,
                ImageUrl = productImage.ImageUrl
            };

            _context.ProductImages.Add(newProductImage);
            await _context.SaveChangesAsync();

            return ToResponseDTO(newProductImage);
        }

        public async Task<ProductImageDTO?> UpdateProductImage(
            int id,
            ProductImageDTO productImage)
        {
            var existingProductImage = await _context.ProductImages.FindAsync(id);

            if (existingProductImage == null)
            {
                return null;
            }

            existingProductImage.ProductId = productImage.ProductId;
            existingProductImage.ImageUrl = productImage.ImageUrl;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProductImageExists(id))
                {
                    return null;
                }

                throw;
            }

            return ToResponseDTO(existingProductImage);
        }

        public async Task<bool> DeleteProductImage(int id)
        {
            var productImage = await _context.ProductImages.FindAsync(id);

            if (productImage == null)
            {
                return false;
            }

            _context.ProductImages.Remove(productImage);
            await _context.SaveChangesAsync();
            return true;
        }

        private static ProductImageDTO ToResponseDTO(ProductImage productImage)
        {
            return new ProductImageDTO
            {
                Id = productImage.Id,
                ProductId = productImage.ProductId,
                ImageUrl = productImage.ImageUrl
            };
        }

        private Task<bool> ProductImageExists(int id)
        {
            return _context.ProductImages.AnyAsync(image => image.Id == id);
        }
    }
}
