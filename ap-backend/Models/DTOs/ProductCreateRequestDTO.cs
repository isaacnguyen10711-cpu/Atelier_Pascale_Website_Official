namespace AtelierPascaleWebsite.Models.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class ProductCreateRequestDTO
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(2000)]
        public string Description { get; set; } = string.Empty;

        [Range(typeof(decimal), "0.01", "999999.99")]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue)]
        public int CategoryId { get; set; }
        public bool IsNewArrival { get; set; } = false;
    }
}
