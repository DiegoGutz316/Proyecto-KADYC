using Proyecto_v1.Data;
using Proyecto_v1.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Proyecto_v1.Services
{
    public interface ICustomerService
    {
        Task<Customer> GetOrCreateCustomerAsync(string email, string userId);
        Task<Customer?> GetCustomerByEmailAsync(string email);
        Task<bool> UpdateCustomerAsync(Customer customer);
    }

    public class CustomerService : ICustomerService
    {
        private readonly ApplicationDBContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public CustomerService(ApplicationDBContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<Customer> GetOrCreateCustomerAsync(string email, string userId)
        {
            // Buscar customer existente por email
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);

            // Si no existe, crear uno nuevo
            if (customer == null)
            {
                var user = await _userManager.FindByIdAsync(userId);
                
                customer = new Customer
                {
                    Email = email,
                    FirstName = user?.UserName ?? "Cliente",
                    LastName = "",
                    Address = "Por completar",
                    IsActive = true
                };

                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            return customer;
        }

        public async Task<Customer?> GetCustomerByEmailAsync(string email)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email == email);
        }

        public async Task<bool> UpdateCustomerAsync(Customer customer)
        {
            try
            {
                _context.Customers.Update(customer);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}