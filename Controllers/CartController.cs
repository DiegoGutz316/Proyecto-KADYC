using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Proyecto_v1.Services;
using System.Security.Claims;

namespace Proyecto_v1.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;

        public CartController(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _cartService.AddToCartAsync(userId, productId, quantity);
            
            if (success)
            {
                TempData["Success"] = "Producto agregado al carrito exitosamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo agregar el producto al carrito.";
            }
            
            return RedirectToAction("ViewProducts", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _cartService.UpdateCartItemAsync(userId, cartItemId, quantity);
            
            if (success)
            {
                TempData["Success"] = "Cantidad actualizada exitosamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo actualizar la cantidad.";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _cartService.RemoveFromCartAsync(userId, cartItemId);
            
            if (success)
            {
                TempData["Success"] = "Producto eliminado del carrito.";
            }
            else
            {
                TempData["Error"] = "No se pudo eliminar el producto.";
            }
            
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> ClearCart()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var success = await _cartService.ClearCartAsync(userId);
            
            if (success)
            {
                TempData["Success"] = "Carrito vaciado exitosamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo vaciar el carrito.";
            }
            
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var count = await _cartService.GetCartItemCountAsync(userId);
            return Json(count);
        }
    }
}