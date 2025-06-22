using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Azure.Messaging;
using AzureFuntions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AzureFuntions;

public class TrakingAlertsEventGridFn
{
    private readonly ILogger<TrakingAlertsEventGridFn> _logger;
    
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _fromEmail;
    private readonly bool _enableSsl;

    public TrakingAlertsEventGridFn(ILogger<TrakingAlertsEventGridFn> logger)
    {
        _logger = logger;
        _smtpHost = "smtp.mailgun.org";
        _smtpPort = 587;
        _smtpUsername = Environment.GetEnvironmentVariable("MailgunUsername") ?? throw new InvalidOperationException("SmtpUsername not configured");
        _smtpPassword = Environment.GetEnvironmentVariable("MailgunPassword") ?? throw new InvalidOperationException("SmtpPassword not configured");
        _fromEmail = "rukayun@gmail.com";
        _enableSsl = false;
    }

    [Function("TrakingAlertsEventGridFn")]
    public async Task Run([EventGridTrigger] CloudEvent cloudEvent)
    {
        _logger.LogInformation("Event type: {type}, Event subject: {subject}", cloudEvent.Type, cloudEvent.Subject);
        try
        {
            if (cloudEvent.Type == "Adopcion.Solicitada")
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var trackingAlert = JsonSerializer.Deserialize<TrakingAlertRequest>(cloudEvent.Data, options);
                
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
            }

        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Error parsing JSON request");
            
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, $"SMTP error sending email");
            
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing tracking alert");
        }
        
    }
}