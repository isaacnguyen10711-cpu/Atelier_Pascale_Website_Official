namespace AtelierPascaleWebsite.Models.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class ItemsInCartUpdateRequestDTO
    {
        [Range(1, 99)]
        public int Quantity { get; set; }
    }
}
