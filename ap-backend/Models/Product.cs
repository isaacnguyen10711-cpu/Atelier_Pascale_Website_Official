namespace AtelierPascaleWebsite.Models;

public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public int CategoryId { get; set; }

    public Category Category { get; set; } = null!;

    public bool IsNewArrival { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();

    public ICollection<ItemsInCart> ItemsInCarts { get; set; } = new List<ItemsInCart>();
    public ICollection<ItemInOrder> ItemsInOrders { get; set; } = new List<ItemInOrder>();

}
