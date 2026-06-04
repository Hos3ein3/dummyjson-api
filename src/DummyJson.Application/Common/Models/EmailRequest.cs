using System.Collections.Generic;

namespace DummyJson.Application.Common.Models;

public class EmailRequest
{
    public string To { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
    
    // Optional: List of CCs or Attachments could be added here
}
