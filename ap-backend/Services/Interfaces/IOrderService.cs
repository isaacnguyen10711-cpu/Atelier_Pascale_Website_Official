using AtelierPascaleWebsite.Models.DTOs;

namespace AtelierPascaleWebsite.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDTO>> GetOrders(int userId, bool isAdmin);
        Task<OrderResponseDTO?> GetOrderById(int id, int userId, bool isAdmin);
        Task<OrderResponseDTO> CreateOrder(int userId, OrderCreateRequestDTO order);
    }
}
