using AtelierPascaleWebsite.Models.DTOs;

namespace AtelierPascaleWebsite.Models.DTOs
{
    public class OrderResponseDTO
    {
        public int OrderId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public ICollection<ItemsInOrderResponseDTO> OrderItems { get; set; } = new List<ItemsInOrderResponseDTO>();
    }
}
