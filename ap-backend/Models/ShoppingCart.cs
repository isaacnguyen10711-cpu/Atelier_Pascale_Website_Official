namespace AtelierPascaleWebsite.Models;

public class ShoppingCart
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public User? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ItemsInCart> ItemsInCarts { get; set; } = new List<ItemsInCart>();
}
