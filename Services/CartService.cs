using Microsoft.EntityFrameworkCore;
using Proyecto_v1.Data;
using Proyecto_v1.Models;

namespace Proyecto_v1.Services
{
    public interface ICartService
    {
        Task<Cart> GetCartByUserIdAsync(string userId);
        Task<bool> AddToCartAsync(string userId, int productId, int quantity = 1);
        Task<bool> UpdateCartItemAsync(string userId, int cartItemId, int quantity);
        Task<bool> RemoveFromCartAsync(string userId, int cartItemId);
        Task<bool> ClearCartAsync(string userId);
        Task<int> GetCartItemCountAsync(string userId);
    }

    public class CartService : ICartService
    {
        private readonly ApplicationDBContext _context;

        public CartService(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<Cart> GetCartByUserIdAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<bool> AddToCartAsync(string userId, int productId, int quantity = 1)
        {
            try
            {
                var product = await _context.Products.FindAsync(productId);
                if (product == null || !product.IsActive || product.Stock < quantity)
                    return false;

                var cart = await GetCartByUserIdAsync(userId);
                
                var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
                
                if (existingItem != null)
                {
                    existingItem.Quantity += quantity;
                    existingItem.UnitPrice = (decimal)product.Price;
                }
                else
                {
                    var cartItem = new CartItem
                    {
                        CartId = cart.Id,
                        ProductId = productId,
                        Quantity = quantity,
                        UnitPrice = (decimal)product.Price,
                        AddedDate = DateTime.Now
                    };
                    _context.CartItems.Add(cartItem);
                }

                cart.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateCartItemAsync(string userId, int cartItemId, int quantity)
        {
            try
            {
                var cart = await GetCartByUserIdAsync(userId);
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                
                if (cartItem == null)
                    return false;

                if (quantity <= 0)
                {
                    _context.CartItems.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity = quantity;
                }

                cart.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveFromCartAsync(string userId, int cartItemId)
        {
            try
            {
                var cart = await GetCartByUserIdAsync(userId);
                var cartItem = cart.CartItems.FirstOrDefault(ci => ci.Id == cartItemId);
                
                if (cartItem == null)
                    return false;

                _context.CartItems.Remove(cartItem);
                cart.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ClearCartAsync(string userId)
        {
            try
            {
                var cart = await GetCartByUserIdAsync(userId);
                _context.CartItems.RemoveRange(cart.CartItems);
                cart.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<int> GetCartItemCountAsync(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            return cart?.TotalItems ?? 0;
        }
    }
}