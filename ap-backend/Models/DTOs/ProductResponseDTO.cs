namespace AtelierPascaleWebsite.Models.DTOs
{
    public class ProductResponseDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public bool IsNewArrival { get; set; }
        public List<ProductImageDTO> Images { get; set; } = new List<ProductImageDTO>();
    }
}
