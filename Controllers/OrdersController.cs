using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto_v1.Data;
using Proyecto_v1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using Proyecto_v1.Services;

namespace Proyecto_v1.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ICustomerService _customerService;

        public OrdersController(ApplicationDBContext context, UserManager<IdentityUser> userManager, ICustomerService customerService)
        {
            _context = context;
            _userManager = userManager;
            _customerService = customerService;
        }

        // M�todo privado para obtener o crear un customer
        private async Task<Customer> GetOrCreateCustomerAsync()
        {
            var userEmail = User.Identity.Name;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            
            return await _customerService.GetOrCreateCustomerAsync(userEmail, userId);
        }

        // GET: Orders - Ver todas las �rdenes del cliente
        public async Task<IActionResult> Index()
        {
            try
            {
                var customer = await GetOrCreateCustomerAsync();

                var orders = await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .Where(o => o.CustomerId == customer.Id)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar las �rdenes. Por favor intenta nuevamente.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Orders/AdminOrders - Ver todas las �rdenes (solo administradores)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminOrders()
        {
            try
            {
                var orders = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                // Calcular estad�sticas
                ViewBag.TotalOrders = orders.Count;
                ViewBag.PendingOrders = orders.Count(o => o.Status == "Pendiente");
                ViewBag.ProcessingOrders = orders.Count(o => o.Status == "Procesando");
                ViewBag.ShippedOrders = orders.Count(o => o.Status == "Enviado");
                ViewBag.DeliveredOrders = orders.Count(o => o.Status == "Entregado");
                ViewBag.CancelledOrders = orders.Count(o => o.Status == "Cancelada");
                ViewBag.TotalRevenue = orders.Where(o => o.Status != "Cancelada").Sum(o => o.Total);

                return View(orders);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar las �rdenes administrativas.";
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: Orders/AdminDetails/5 - Ver detalles de orden (vista administrativa)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los detalles de la orden.";
                return RedirectToAction("AdminOrders");
            }
        }

        // GET: Orders/AdminInvoice/5 - Ver factura administrativa
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminInvoice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .FirstOrDefaultAsync(o => o.Id == id);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la factura administrativa.";
                return RedirectToAction("AdminOrders");
            }
        }

        // POST: Orders/UpdateOrderStatus - Actualizar estado de orden (solo administradores)
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order == null)
                {
                    TempData["Error"] = "Orden no encontrada.";
                    return RedirectToAction("AdminOrders");
                }

                var validStatuses = new[] { "Pendiente", "Procesando", "Enviado", "Entregado", "Cancelada" };
                if (!validStatuses.Contains(newStatus))
                {
                    TempData["Error"] = "Estado inv�lido.";
                    return RedirectToAction("AdminOrders");
                }

                // Si se cancela una orden, restaurar el stock
                if (newStatus == "Cancelada" && order.Status != "Cancelada")
                {
                    foreach (var detail in order.OrderDetails)
                    {
                        detail.Product.Stock += detail.Quantity;
                        _context.Products.Update(detail.Product);
                    }
                }

                order.Status = newStatus;
                _context.Orders.Update(order);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Estado de la orden #{order.Id.ToString("D6")} actualizado a {newStatus}.";
                return RedirectToAction("AdminOrders");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el estado de la orden.";
                return RedirectToAction("AdminOrders");
            }
        }

        // GET: Orders/Details/5 - Ver detalles de una orden espec�fica
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var customer = await GetOrCreateCustomerAsync();

                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer.Id);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar los detalles de la orden.";
                return RedirectToAction("Index");
            }
        }

        // POST: Orders/CreateFromCart - Crear orden desde el carrito
        [HttpPost]
        public async Task<IActionResult> CreateFromCart()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var customer = await GetOrCreateCustomerAsync();

                // Verificar que el customer tenga informaci�n completa
                if (string.IsNullOrEmpty(customer.FirstName) || customer.FirstName == "Cliente" ||
                    string.IsNullOrEmpty(customer.Address) || customer.Address == "Por completar")
                {
                    TempData["Warning"] = "Por favor, completa tu perfil antes de realizar una compra. Te hemos redirigido a la p�gina de perfil.";
                    return RedirectToAction("Profile");
                }

                // Obtener el carrito del usuario
                var cart = await _context.Carts
                    .Include(c => c.CartItems)
                        .ThenInclude(ci => ci.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);

                if (cart == null || !cart.CartItems.Any())
                {
                    TempData["Error"] = "El carrito est� vac�o.";
                    return RedirectToAction("Index", "Cart");
                }

                // Verificar stock disponible
                foreach (var cartItem in cart.CartItems)
                {
                    if (cartItem.Product.Stock < cartItem.Quantity)
                    {
                        TempData["Error"] = $"No hay suficiente stock para el producto {cartItem.Product.Name}. Stock disponible: {cartItem.Product.Stock}";
                        return RedirectToAction("Index", "Cart");
                    }
                }

                // Crear la orden
                var order = new Order
                {
                    OrderDate = DateTime.Now,
                    DevolutionDate = DateTime.Now.AddDays(30), // 30 d�as para devoluci�n
                    SubTotal = cart.TotalAmount,
                    IVA = cart.TotalAmount * 0.13m, // 13% de IVA en Costa Rica
                    Total = cart.TotalAmount * 1.13m,
                    Status = "Pendiente",
                    CustomerId = customer.Id,
                    OrderDetails = new List<OrderDetail>()
                };

                // Crear detalles de la orden
                foreach (var cartItem in cart.CartItems)
                {
                    var orderDetail = new OrderDetail
                    {
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        Price = cartItem.UnitPrice,
                        SubTotal = cartItem.TotalPrice
                    };
                    order.OrderDetails.Add(orderDetail);

                    // Reducir el stock del producto
                    cartItem.Product.Stock -= cartItem.Quantity;
                    _context.Products.Update(cartItem.Product);
                }

                // Guardar la orden
                _context.Orders.Add(order);

                // Limpiar el carrito
                _context.CartItems.RemoveRange(cart.CartItems);

                await _context.SaveChangesAsync();

                TempData["Success"] = "�Tu pedido ha sido creado exitosamente!";
                return RedirectToAction("Details", new { id = order.Id });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurri� un error al procesar tu pedido. Por favor intenta nuevamente.";
                return RedirectToAction("Index", "Cart");
            }
        }

        // GET: Orders/Invoice/5 - Generar factura en PDF o vista
        public async Task<IActionResult> Invoice(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var customer = await GetOrCreateCustomerAsync();

                var order = await _context.Orders
                    .Include(o => o.Customer)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                            .ThenInclude(p => p.Category)
                    .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer.Id);

                if (order == null)
                {
                    return NotFound();
                }

                return View(order);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar la factura.";
                return RedirectToAction("Details", new { id = id });
            }
        }

        // POST: Orders/CancelOrder - Cancelar una orden (solo si est� pendiente)
        [HttpPost]
        public async Task<IActionResult> CancelOrder(int id)
        {
            try
            {
                var customer = await GetOrCreateCustomerAsync();

                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .FirstOrDefaultAsync(o => o.Id == id && o.CustomerId == customer.Id);

                if (order == null)
                {
                    return NotFound();
                }

                if (order.Status != "Pendiente")
                {
                    TempData["Error"] = "Solo se pueden cancelar �rdenes pendientes.";
                    return RedirectToAction("Details", new { id = order.Id });
                }

                // Restaurar el stock de los productos
                foreach (var detail in order.OrderDetails)
                {
                    detail.Product.Stock += detail.Quantity;
                    _context.Products.Update(detail.Product);
                }

                // Cambiar el estado de la orden
                order.Status = "Cancelada";
                _context.Orders.Update(order);

                await _context.SaveChangesAsync();

                TempData["Success"] = "La orden ha sido cancelada exitosamente.";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Ocurri� un error al cancelar la orden.";
                return RedirectToAction("Details", new { id = id });
            }
        }

        // GET: Orders/Profile - Ver y editar perfil del cliente
        public async Task<IActionResult> Profile()
        {
            try
            {
                var customer = await GetOrCreateCustomerAsync();
                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al cargar el perfil.";
                return RedirectToAction("Index", "Home");
            }
        }

        // POST: Orders/UpdateProfile - Actualizar perfil del cliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(Customer model)
        {
            try
            {
                var customer = await GetOrCreateCustomerAsync();

                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(model.FirstName))
                {
                    ModelState.AddModelError("FirstName", "El nombre es requerido.");
                }

                if (string.IsNullOrWhiteSpace(model.Address))
                {
                    ModelState.AddModelError("Address", "La direcci�n es requerida.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                // Actualizar campos
                customer.FirstName = model.FirstName.Trim();
                customer.LastName = model.LastName?.Trim() ?? "";
                customer.Address = model.Address.Trim();

                var success = await _customerService.UpdateCustomerAsync(customer);

                if (success)
                {
                    TempData["Success"] = "Perfil actualizado exitosamente. �Ya puedes realizar compras!";
                    return RedirectToAction("Profile");
                }
                else
                {
                    TempData["Error"] = "Error al actualizar el perfil.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error al actualizar el perfil.";
                return View(model);
            }
        }
    }
}