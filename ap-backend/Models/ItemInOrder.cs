namespace AtelierPascaleWebsite.Models
{
    public class ItemInOrder
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; } = null;
        public int OrderId { get; set; }
        public Order? Order { get; set; } = null;
        public int Quantity { get; set; }
        public decimal PriceAtPurchase { get; set; }

    }
}
