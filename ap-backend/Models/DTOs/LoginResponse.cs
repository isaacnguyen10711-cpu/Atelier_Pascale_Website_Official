namespace AtelierPascaleWebsite.Models.DTOs
{
    public class LoginResponse
    {
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;

        public int ExpiresIn { get; set; } 
    }
}
