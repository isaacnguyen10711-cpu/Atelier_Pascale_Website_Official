using AtelierPascaleWebsite.Data;
using AtelierPascaleWebsite.Models;
using AtelierPascaleWebsite.Models.DTOs;
using AtelierPascaleWebsite.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AtelierPascaleWebsite.Services
{
    public class ItemsInCartService : IItemsInCartService
    {
        private readonly DatabaseContext _context;

        public ItemsInCartService(DatabaseContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ItemsInCartResponseDTO>> GetItemsInCart(int userId)
        {
            return await _context.ItemsInCarts
                .Where(item => item.ShoppingCart != null && item.ShoppingCart.UserId == userId)
                .Select(item => new ItemsInCartResponseDTO
                {
                    Id = item.Id,
                    ProductId = item.Product!.Id,
                    ProductName = item.Product.Name,
                    CategoryName = item.Product.Category.Name,
                    Price = item.Product.Price,
                    Quantity = item.Quantity,
                    ProductImageUrl = item.Product.Images
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault() ?? string.Empty
                })
                .ToListAsync();
        }

        public async Task<bool> UpdateItemQuantity(
            int id,
            int userId,
            int quantity)
        {
            var existingItem = await _context.ItemsInCarts
                .Where(item => item.ShoppingCart != null &&
                    item.ShoppingCart.UserId == userId)
                .FirstOrDefaultAsync(item => item.Id == id);

            if (existingItem == null)
            {
                return false;
            }

            existingItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ItemsInCartResponseDTO?> AddItemToCart(
            int userId,
            ItemsInCartCreateRequestDTO item)
        {
            var product = await _context.Products
                .Include(product => product.Category)
                .Include(product => product.Images)
                .FirstOrDefaultAsync(product => product.Id == item.ProductId);

            if (product == null)
            {
                return null;
            }

            var shoppingCart = await _context.ShoppingCarts
                .Include(cart => cart.ItemsInCarts)
                .FirstOrDefaultAsync(cart => cart.UserId == userId);

            if (shoppingCart == null)
            {
                return null;
            }

            var existingItem = shoppingCart.ItemsInCarts
                .FirstOrDefault(cartItem => cartItem.ProductId == item.ProductId);

            if (existingItem != null)
            {
                existingItem.Quantity += 1;
                await _context.SaveChangesAsync();
                return new ItemsInCartResponseDTO
                {
                    Id = existingItem.Id,
                    ProductId = product.Id,
                    ProductName = product.Name,
                    CategoryName = product.Category.Name,
                    Price = product.Price,
                    Quantity = existingItem.Quantity,
                    ProductImageUrl = product.Images
                        .Select(image => image.ImageUrl)
                        .FirstOrDefault() ?? string.Empty
                };
            }

            var newItem = new ItemsInCart
            {
                ShoppingCartId = shoppingCart.Id,
                ProductId = item.ProductId,
                Quantity = 1
            };

            _context.ItemsInCarts.Add(newItem);
            await _context.SaveChangesAsync();

            return new ItemsInCartResponseDTO
            {
                Id = newItem.Id,
                ProductId = product.Id,
                ProductName = product.Name,
                CategoryName = product.Category.Name,
                Price = product.Price,
                Quantity = newItem.Quantity,
                ProductImageUrl = product.Images
                    .Select(image => image.ImageUrl)
                    .FirstOrDefault() ?? string.Empty
            };
        }

        public async Task<bool> DeleteItemFromCart(int id, int userId)
        {
            var item = await _context.ItemsInCarts
                .Where(cartItem => cartItem.ShoppingCart != null &&
                    cartItem.ShoppingCart.UserId == userId)
                .FirstOrDefaultAsync(cartItem => cartItem.Id == id);

            if (item == null)
            {
                return false;
            }

            _context.ItemsInCarts.Remove(item);
            await _context.SaveChangesAsync();
            return true;
        }

    }
}
