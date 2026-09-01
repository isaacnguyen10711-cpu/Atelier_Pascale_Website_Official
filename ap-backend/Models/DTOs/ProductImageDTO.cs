namespace AtelierPascaleWebsite.Models.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class ProductImageDTO
    {
        public int Id { get; set; }
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        [Required]
        [Url]
        [StringLength(2048)]
        public string ImageUrl { get; set; } = string.Empty;
    }
}
