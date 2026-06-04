using System.Threading;
using System.Threading.Tasks;
using DummyJson.Application.Common.Models;

namespace DummyJson.Application.Common.Interfaces;

public interface ISmsService
{
    Task SendSmsAsync(SmsRequest request, CancellationToken cancellationToken = default);
}
