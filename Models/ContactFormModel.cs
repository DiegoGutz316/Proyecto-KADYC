using System.ComponentModel.DataAnnotations;

namespace Proyecto_v1.Models
{
    public class ContactFormModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
        [Display(Name = "Nombre")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido")]
        [Display(Name = "Correo Electrónico")]
        public string? Correo { get; set; }

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [Phone(ErrorMessage = "Ingresa un número de teléfono válido")]
        [StringLength(20, MinimumLength = 7, ErrorMessage = "El teléfono debe tener entre 7 y 20 caracteres")]
        [Display(Name = "Teléfono")]
        public string? Telefono { get; set; }

        [Required(ErrorMessage = "El mensaje es obligatorio")]
        [StringLength(1000, MinimumLength = 10, ErrorMessage = "El mensaje debe tener entre 10 y 1000 caracteres")]
        [Display(Name = "Mensaje")]
        public string? Mensaje { get; set; }

        // Campo honeypot para detectar spam
        public string? Company { get; set; }

        // Información adicional
        public DateTime FechaEnvio { get; set; } = DateTime.Now;
        public string? UsuarioIP { get; set; }
        public string? UserAgent { get; set; }
    }

    public class ContactFormResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string> Errors { get; set; } = new();
    }
}