namespace AtelierPascaleWebsite.Models.DTOs
{
    using System.ComponentModel.DataAnnotations;

    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        [StringLength(254)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}
