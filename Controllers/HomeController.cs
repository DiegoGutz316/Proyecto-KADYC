using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Proyecto_v1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Proyecto_v1.Data;

namespace Proyecto_v1.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDBContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                // Obtener productos destacados para la landing page
                var featuredProducts = await _context.Products
                    .Include(p => p.Category)
                    .Where(p => p.IsActive)
                    .OrderByDescending(p => p.Id) // Los más recientes
                    .Take(6) // Solo 6 productos destacados
                    .ToListAsync();

                return View(featuredProducts);
            }
            catch
            {
                // En caso de error, devolver vista vacía
                return View(new List<Product>());
            }
        }

        // Acción pública para ver detalles del producto
        public async Task<IActionResult> DetallesProducto(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);
            
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        public IActionResult Estilos()
        {
            return View();
        }

        [Authorize] // Solo usuarios autenticados pueden ver productos
        public async Task<IActionResult> ViewProducts()
        {
            var productos = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .ToListAsync();

            return View(productos);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
