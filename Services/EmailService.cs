using System.Net;
using System.Net.Mail;
using Proyecto_v1.Models;

namespace Proyecto_v1.Services
{
    public interface IEmailService
    {
      Task<bool> SendContactEmailAsync(ContactFormModel contactForm);
    Task<bool> SendEmailAsync(string toEmail, string subject, string message);
 }

    public class EmailService : IEmailService
    {
      private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

      public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
        _configuration = configuration;
  _logger = logger;
        }

        public async Task<bool> SendContactEmailAsync(ContactFormModel contactForm)
        {
 try
            {
    var subject = $"Nuevo mensaje de contacto de {contactForm.Nombre} - KADYC";
  var body = GenerateContactEmailBody(contactForm);
var companyEmail = _configuration["EmailSettings:CompanyEmail"] ?? "info@kadyc.com";

             return await SendEmailAsync(companyEmail, subject, body);
      }
      catch (Exception ex)
            {
         _logger.LogError(ex, "Error al enviar correo de contacto");
        return false;
   }
        }

 public async Task<bool> SendEmailAsync(string toEmail, string subject, string message)
        {
        try
            {
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
      var smtpUsername = _configuration["EmailSettings:SmtpUsername"];
  var smtpPassword = _configuration["EmailSettings:SmtpPassword"];
          var fromEmail = _configuration["EmailSettings:FromEmail"];
      var fromName = _configuration["EmailSettings:FromName"] ?? "KADYC";

        using var smtpClient = new SmtpClient(smtpServer, smtpPort);
   smtpClient.EnableSsl = true;
    smtpClient.UseDefaultCredentials = false;
     smtpClient.Credentials = new NetworkCredential(smtpUsername, smtpPassword);

      using var mailMessage = new MailMessage();
        mailMessage.From = new MailAddress(fromEmail!, fromName);
        mailMessage.To.Add(toEmail);
                mailMessage.Subject = subject;
   mailMessage.Body = message;
 mailMessage.IsBodyHtml = true;
          mailMessage.Priority = MailPriority.Normal;

     await smtpClient.SendMailAsync(mailMessage);
        _logger.LogInformation("Correo enviado exitosamente a {Email}", toEmail);
      return true;
  }
catch (SmtpException smtpEx)
            {
     _logger.LogError(smtpEx, "Error SMTP al enviar correo a {Email}: {Error}", toEmail, smtpEx.Message);
      return false;
      }
      catch (Exception ex)
    {
           _logger.LogError(ex, "Error general al enviar correo a {Email}", toEmail);
         return false;
            }
        }

        private string GenerateContactEmailBody(ContactFormModel contactForm)
   {
       return $@"
<!DOCTYPE html>
<html>
<head>
 <meta charset='UTF-8'>
    <style>
   body {{
      font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
       line-height: 1.6;
  color: #333;
            max-width: 600px;
            margin: 0 auto;
 padding: 20px;
    }}
        .header {{
      background: linear-gradient(135deg, #007bff 0%, #0056b3 100%);
        color: white;
            padding: 20px;
  border-radius: 10px 10px 0 0;
    text-align: center;
        }}
        .content {{
  background: #f8f9fa;
padding: 30px;
  border-radius: 0 0 10px 10px;
      }}
        .field {{
            margin-bottom: 15px;
    padding: 10px;
 background: white;
   border-radius: 5px;
    border-left: 4px solid #007bff;
        }}
.field-label {{
    font-weight: bold;
        color: #007bff;
            margin-bottom: 5px;
  }}
        .field-value {{
     color: #333;
  word-wrap: break-word;
    }}
  .footer {{
            margin-top: 20px;
     text-align: center;
 color: #6c757d;
            font-size: 12px;
    }}
        .urgent {{
            background: #fff3cd;
 border: 1px solid #ffeaa7;
     padding: 10px;
            border-radius: 5px;
            margin-bottom: 20px;
        }}
    </style>
</head>
<body>
    <div class='header'>
        <h1>?? KADYC</h1>
        <h2>Nuevo Mensaje de Contacto</h2>
    </div>
    
    <div class='content'>
        <div class='urgent'>
 <strong>?? Nuevo mensaje recibido a las {contactForm.FechaEnvio:dddd, dd MMMM yyyy 'a las' HH:mm}</strong>
        </div>
        
        <div class='field'>
    <div class='field-label'>?? Nombre:</div>
            <div class='field-value'>{contactForm.Nombre}</div>
        </div>
        
<div class='field'>
            <div class='field-label'>?? Correo Electrónico:</div>
            <div class='field-value'><a href='mailto:{contactForm.Correo}'>{contactForm.Correo}</a></div>
        </div>
    
        <div class='field'>
            <div class='field-label'>?? Teléfono:</div>
            <div class='field-value'><a href='tel:{contactForm.Telefono}'>{contactForm.Telefono}</a></div>
        </div>
        
        <div class='field'>
    <div class='field-label'>?? Mensaje:</div>
            <div class='field-value'>{contactForm.Mensaje}</div>
     </div>
        
    <div class='field'>
            <div class='field-label'>?? Información Técnica:</div>
            <div class='field-value'>
       IP: {contactForm.UsuarioIP}<br>
           Fecha: {contactForm.FechaEnvio:dd/MM/yyyy HH:mm:ss}<br>
              User Agent: {contactForm.UserAgent}
     </div>
  </div>
    </div>
    
    <div class='footer'>
        <p>?? Este mensaje fue enviado desde el formulario de contacto de KADYC</p>
        <p>Para responder, simplemente contesta a este correo o contacta directamente a <strong>{contactForm.Correo}</strong></p>
        <hr>
        <p style='color: #007bff; font-weight: bold;'>KADYC - THE NEW</p>
    </div>
</body>
</html>";
        }
  }
}