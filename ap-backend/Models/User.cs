namespace AtelierPascaleWebsite.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public string Role { get; set; } = "Customer";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ShoppingCart? ShoppingCart { get; set; }
    public ICollection<Order> Orders { get; set; } = new List<Order>();

}
