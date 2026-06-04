namespace DummyJson.Infrastructure.Services;

public class SmsSettings
{
    public const string SectionName = "SmsSettings";
    
    public string AccountSid { get; set; } = string.Empty;
    public string AuthToken { get; set; } = string.Empty;
    public string FromPhoneNumber { get; set; } = string.Empty;
}
