using AtelierPascaleWebsite.Data;
using AtelierPascaleWebsite.Models;
using AtelierPascaleWebsite.Models.DTOs;
using AtelierPascaleWebsite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtelierPascaleWebsite.Services
{
    public class OrderService : IOrderService
    {
        private readonly DatabaseContext _context;
        private readonly EmailSender _emailSender;

        public OrderService(DatabaseContext context, EmailSender emailSender)
        {
            _context = context;
            _emailSender = emailSender;
        }

        public async Task<IEnumerable<OrderResponseDTO>> GetOrders(
            int userId,
            bool isAdmin)
        {


            var ordersQuery = _context.Orders
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.Product)
                        .ThenInclude(product => product!.Category)
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.Product)
                        .ThenInclude(product => product!.Images)
                .AsQueryable();

            if (!isAdmin)
            {
                ordersQuery = ordersQuery.Where(order => order.UserId == userId);
            }

            var orders = await ordersQuery
                .OrderByDescending(order => order.OrderDate)
                .ToListAsync();

            return orders.Select(order => new OrderResponseDTO
            {
                OrderId = order.Id,
                Email = order.Email,
                FirstName = order.FirstName,
                LastName = order.LastName,
                ShippingAddress = order.ShippingAddress,
                City = order.City,
                State = order.State,
                PostalCode = order.PostalCode,
                Status = order.Status,
                TotalPrice = order.TotalPrice,
                OrderDate = order.OrderDate,
                OrderItems = order.OrderItems.Select(item => new ItemsInOrderResponseDTO
                {
                    ProductId = item.ProductId,
                    OrderId = item.OrderId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.PriceAtPurchase,
                    ProductName = item.Product!.Name,
                    CategoryName = item.Product!.Category.Name,
                    ImageUrl = item.Product.Images
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault() ?? string.Empty
                }).ToList() 
            }).ToList();
        }

        public async Task<OrderResponseDTO?> GetOrderById(
            int id,
            int userId,
            bool isAdmin)
        {
            var order = await _context.Orders
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.Product)
                        .ThenInclude(product => product!.Category)
                .Include(order => order.OrderItems)
                    .ThenInclude(item => item.Product)
                        .ThenInclude(product => product!.Images)
                .FirstOrDefaultAsync(order => order.Id == id);

            if (order == null)
            {
                return null;
            }

            if (!isAdmin && order.UserId != userId)
            {
                throw new UnauthorizedAccessException();
            }

            return new OrderResponseDTO
            {
                OrderId = order.Id,
                Email = order.Email,
                FirstName = order.FirstName,
                LastName = order.LastName,
                ShippingAddress = order.ShippingAddress,
                City = order.City,
                State = order.State,
                PostalCode = order.PostalCode,
                Status = order.Status,
                TotalPrice = order.TotalPrice,
                OrderDate = order.OrderDate,
                OrderItems = order.OrderItems.Select(item => new ItemsInOrderResponseDTO
                {
                    ProductId = item.ProductId,
                    OrderId = item.OrderId,
                    Quantity = item.Quantity,
                    PriceAtPurchase = item.PriceAtPurchase,
                    ProductName = item.Product!.Name,
                    CategoryName = item.Product!.Category.Name,
                    ImageUrl = item.Product.Images
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault() ?? string.Empty
                }).ToList()
            };
        }

        public async Task<OrderResponseDTO> CreateOrder(
            int userId,
            OrderCreateRequestDTO order)
        {
            var shoppingCart = await _context.ShoppingCarts
                .Include(cart => cart.ItemsInCarts)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(cart => cart.UserId == userId);

            if (shoppingCart == null || shoppingCart.ItemsInCarts.Count == 0)
            {
                throw new InvalidOperationException(
                    "Shopping cart is empty or does not exist.");
            }

            var totalPrice = shoppingCart.ItemsInCarts.Sum(
                item => item.Product!.Price * item.Quantity);

            var confirmedOrder = new Order
            {
                UserId = userId,
                FirstName = order.FirstName,
                LastName = order.LastName,
                Email = order.Email,
                ShippingAddress = order.ShippingAddress,
                City = order.City,
                State = order.State,
                PostalCode = order.PostalCode,
                TotalPrice = totalPrice,
                OrderDate = DateTime.UtcNow,
                Status = "Pending"
            };

            _context.Orders.Add(confirmedOrder);
            await _context.SaveChangesAsync();

            foreach (var cartItem in shoppingCart.ItemsInCarts)
            {
                _context.ItemsInOrders.Add(new ItemInOrder
                {
                    OrderId = confirmedOrder.Id,
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    PriceAtPurchase = cartItem.Product!.Price
                });
            }

            await _context.SaveChangesAsync();

            _context.ItemsInCarts.RemoveRange(shoppingCart.ItemsInCarts);
            await _context.SaveChangesAsync();

            var emailBody = $@"
            <h2>Dear {confirmedOrder.FirstName},</h2>
            <p>Thank you for your order!</p>
            <p>Your order ID is <strong>{confirmedOrder.Id}</strong>.</p>
            <p>Total price: <strong>{confirmedOrder.TotalPrice:C}</strong></p>
            <p>We will notify you once your order is shipped.</p>
            <br />
            <p>Best regards,<br />Atelier Pascale</p>";

            await _emailSender.SendEmailAsync(
                confirmedOrder.FirstName,
                confirmedOrder.Email,
                "Order Confirmation",
                emailBody);

            return new OrderResponseDTO
            {
                OrderId = confirmedOrder.Id,
                Email = confirmedOrder.Email,
                FirstName = confirmedOrder.FirstName,
                LastName = confirmedOrder.LastName,
                ShippingAddress = confirmedOrder.ShippingAddress,
                City = confirmedOrder.City,
                State = confirmedOrder.State,
                PostalCode = confirmedOrder.PostalCode,
                Status = confirmedOrder.Status,
                TotalPrice = confirmedOrder.TotalPrice,
                OrderDate = confirmedOrder.OrderDate
            };
        }
    }
}
