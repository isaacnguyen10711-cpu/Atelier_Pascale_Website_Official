namespace AtelierPascaleWebsite.Models;

public class ItemsInCart
{
    public int Id { get; set; }

    public int ShoppingCartId { get; set; }

    public ShoppingCart? ShoppingCart { get; set; }

    public int ProductId { get; set; }

    public Product? Product { get; set; }

    public int Quantity { get; set; } = 1;
}
