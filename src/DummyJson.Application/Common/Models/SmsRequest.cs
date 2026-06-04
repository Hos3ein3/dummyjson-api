namespace DummyJson.Application.Common.Models;

public class SmsRequest
{
    public string ToPhoneNumber { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
