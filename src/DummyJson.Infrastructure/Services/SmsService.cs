using System;
using System.Threading;
using System.Threading.Tasks;
using DummyJson.Application.Common.Interfaces;
using DummyJson.Application.Common.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace DummyJson.Infrastructure.Services;

public class SmsService : ISmsService
{
    private readonly SmsSettings _settings;
    private readonly ILogger<SmsService> _logger;

    public SmsService(IOptions<SmsSettings> settings, ILogger<SmsService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendSmsAsync(SmsRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            TwilioClient.Init(_settings.AccountSid, _settings.AuthToken);

            var message = await MessageResource.CreateAsync(
                body: request.Message,
                from: new PhoneNumber(_settings.FromPhoneNumber),
                to: new PhoneNumber(request.ToPhoneNumber)
            );

            _logger.LogInformation("SMS sent successfully to {To}. Twilio SID: {Sid}", request.ToPhoneNumber, message.Sid);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while sending SMS to {To}", request.ToPhoneNumber);
            throw;
        }
    }
}
