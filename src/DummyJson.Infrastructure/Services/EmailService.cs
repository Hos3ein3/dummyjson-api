using System;
using System.Threading;
using System.Threading.Tasks;
using DummyJson.Application.Common.Interfaces;
using DummyJson.Application.Common.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace DummyJson.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(EmailRequest request, CancellationToken cancellationToken = default)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        message.To.Add(new MailboxAddress(request.To, request.To));
        message.Subject = request.Subject;

        var bodyBuilder = new BodyBuilder();
        if (request.IsHtml)
        {
            bodyBuilder.HtmlBody = request.Body;
        }
        else
        {
            bodyBuilder.TextBody = request.Body;
        }

        message.Body = bodyBuilder.ToMessageBody();

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(_settings.SmtpServer, _settings.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_settings.SmtpUsername, _settings.SmtpPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}", request.To);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending email to {To}", request.To);
            throw;
        }
    }
}
