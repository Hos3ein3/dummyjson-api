using System.Threading;
using System.Threading.Tasks;
using DummyJson.Application.Common.Models;

namespace DummyJson.Application.Common.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(EmailRequest request, CancellationToken cancellationToken = default);
}
