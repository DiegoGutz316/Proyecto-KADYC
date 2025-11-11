using Microsoft.AspNetCore.Mvc;
using Proyecto_v1.Models;
using Proyecto_v1.Services;
using System.ComponentModel.DataAnnotations;

namespace Proyecto_v1.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<ContactController> _logger;

        public ContactController(IEmailService emailService, ILogger<ContactController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendContactForm([FromBody] ContactFormModel model)
        {
            try
            {
                // Verificar si es spam usando honeypot
                if (!string.IsNullOrWhiteSpace(model.Company))
                {
                    _logger.LogWarning("Potencial spam detectado desde IP {IP}", GetClientIpAddress());
                    return BadRequest(new ContactFormResponse
                    {
                        Success = false,
                        Message = "Error en el envío. Por favor, inténtalo nuevamente."
                    });
                }

                // Validar el modelo
                if (!ModelState.IsValid)
                {
                    var errors = ModelState
              .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                            kvp => kvp.Value?.Errors.First().ErrorMessage ?? "Error de validación"
                       );

                    return BadRequest(new ContactFormResponse
                    {
                        Success = false,
                        Message = "Por favor, corrige los errores en el formulario.",
                        Errors = errors
                    });
                }

                // Completar información adicional
                model.UsuarioIP = GetClientIpAddress();
                model.UserAgent = Request.Headers.UserAgent.ToString();
                model.FechaEnvio = DateTime.Now;

                // Enviar el correo
                var emailSent = await _emailService.SendContactEmailAsync(model);

                if (emailSent)
                {
                    _logger.LogInformation("Formulario de contacto enviado exitosamente desde {IP} por {Email}",
                 model.UsuarioIP, model.Correo);

                    return Ok(new ContactFormResponse
                    {
                        Success = true,
                        Message = "¡Mensaje enviado exitosamente! Nos pondremos en contacto contigo pronto. 🌟"
                    });
                }
                else
                {
                    _logger.LogError("Error al enviar correo de contacto desde {IP} por {Email}",
              model.UsuarioIP, model.Correo);

                    return StatusCode(500, new ContactFormResponse
                    {
                        Success = false,
                        Message = "Hubo un problema al enviar tu mensaje. Por favor, inténtalo más tarde o contáctanos directamente."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error inesperado al procesar formulario de contacto");

                return StatusCode(500, new ContactFormResponse
                {
                    Success = false,
                    Message = "Error interno del servidor. Por favor, inténtalo más tarde."
                });
            }
        }

        [HttpGet("test")]
        public async Task<IActionResult> TestEmail()
        {
            try
            {
                // Solo permitir en desarrollo
                if (!HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment())
                {
                    return NotFound();
                }

                var testModel = new ContactFormModel
                {
                    Nombre = "Test Usuario",
                    Correo = "test@example.com",
                    Telefono = "+506 1234-5678",
                    Mensaje = "Este es un mensaje de prueba desde el sistema de contacto de KADYC.",
                    UsuarioIP = GetClientIpAddress(),
                    UserAgent = Request.Headers.UserAgent.ToString(),
                    FechaEnvio = DateTime.Now
                };

                var emailSent = await _emailService.SendContactEmailAsync(testModel);

                return Ok(new { success = emailSent, message = emailSent ? "Email de prueba enviado" : "Error al enviar email de prueba" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en test de email");
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        private string GetClientIpAddress()
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress;

            // Verificar headers de proxy
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
            {
                var forwarded = Request.Headers["X-Forwarded-For"].ToString();
                if (!string.IsNullOrEmpty(forwarded))
                {
                    return forwarded.Split(',').First().Trim();
                }
            }

            if (Request.Headers.ContainsKey("X-Real-IP"))
            {
                return Request.Headers["X-Real-IP"].ToString();
            }

            return ipAddress?.ToString() ?? "Unknown";
        }
    }
}