using AtelierPascaleWebsite.Models.DTOs;

namespace AtelierPascaleWebsite.Services.Interfaces
{
    public interface IItemsInCartService
    {
        Task<IEnumerable<ItemsInCartResponseDTO>> GetItemsInCart(int userId);
        Task<bool> UpdateItemQuantity(int id, int userId, int quantity);
        Task<ItemsInCartResponseDTO?> AddItemToCart(
            int userId,
            ItemsInCartCreateRequestDTO item);
        Task<bool> DeleteItemFromCart(int id, int userId);
    }
}
