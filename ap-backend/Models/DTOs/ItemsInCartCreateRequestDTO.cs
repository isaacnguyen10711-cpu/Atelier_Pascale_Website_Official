namespace AtelierPascaleWebsite.Models.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class ItemsInCartCreateRequestDTO
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }
    }
}
