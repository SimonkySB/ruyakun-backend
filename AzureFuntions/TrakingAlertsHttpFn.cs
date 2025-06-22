using System.Net;
using System.Net.Mail;
using System.Text.Json;
using AzureFuntions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AzureFuntions;

public class TrakingAlertsHttpFn
{
    private readonly ILogger<TrakingAlertsHttpFn> _logger;
    
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly bool _enableSsl;

    public TrakingAlertsHttpFn(ILogger<TrakingAlertsHttpFn> logger)
    {
        _logger = logger;
        _smtpHost = "smtp.mailgun.org";
        _smtpPort = 587;
        _smtpUsername = Environment.GetEnvironmentVariable("MailgunUsername") ?? throw new InvalidOperationException("SmtpUsername not configured");
        _smtpPassword = Environment.GetEnvironmentVariable("MailgunPassword") ?? throw new InvalidOperationException("SmtpPassword not configured");
        _fromEmail = "rukayun@gmail.com";
        _enableSsl = false;
    }

    [Function("TrakingAlertsHttpFn")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        try
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            if (string.IsNullOrEmpty(requestBody))
            {
                _logger.LogWarning("Request body is empty");
                return new BadRequestObjectResult("Request body is required");
            }
            
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var trackingAlert = JsonSerializer.Deserialize<TrakingAlertRequest>(requestBody, options);
            
            using var smtpClient = new SmtpClient(_smtpHost, _smtpPort)
            {
                Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
                EnableSsl = _enableSsl,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Timeout = 30000
            };
            
            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, "Sistema de Alertas, Rukayun"),
                Subject = trackingAlert?.asunto ?? "",
                Body = trackingAlert?.contenido,
                IsBodyHtml = true,
                Priority = MailPriority.Normal
            };

            string emailAdoptante = trackingAlert?.emailAdoptante ?? "";
            
            mailMessage.To.Add(new MailAddress(emailAdoptante));
            
            _logger.LogInformation($"Sending email to {emailAdoptante}");
            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email sent successfully to {emailAdoptante}");
            
            return new OkObjectResult("Message was sent successfully!");
            
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing JSON request");
            return new BadRequestObjectResult("Invalid JSON format");
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, $"SMTP error sending email: {ex.Message}");
            return new BadRequestObjectResult($"SMTP error sending email");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing tracking alert");
            return new BadRequestObjectResult("Error processing tracking alert");
        }
        
            
        
        
    }

}