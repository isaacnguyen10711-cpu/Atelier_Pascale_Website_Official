using AtelierPascaleWebsite.Models.DTOs;

namespace AtelierPascaleWebsite.Services.Interfaces
{
    public interface IProductImageService
    {
        Task<IEnumerable<ProductImageDTO>> GetAllProductImages();
        Task<ProductImageDTO?> GetProductImageById(int id);
        Task<ProductImageDTO?> CreateProductImage(ProductImageDTO productImage);
        Task<ProductImageDTO?> UpdateProductImage(int id, ProductImageDTO productImage);
        Task<bool> DeleteProductImage(int id);
    }
}
