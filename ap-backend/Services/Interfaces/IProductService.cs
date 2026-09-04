using AtelierPascaleWebsite.Models.DTOs;

namespace AtelierPascaleWebsite.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDTO>> GetAllProducts(string? sortBy);

        Task<ProductResponseDTO?> GetProductById(int id);

        Task<IEnumerable<ProductResponseDTO>> GetProductsByCategory(string categoryName, string? sortBy);

        Task<ProductResponseDTO?> CreateProduct(ProductCreateRequestDTO productRequest);

        Task<ProductResponseDTO?> UpdateProduct(int id, ProductUpdateRequestDTO productRequest);

        Task<bool> DeleteProduct(int id);

    }
}

