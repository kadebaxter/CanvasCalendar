using Microsoft.Extensions.Configuration;

namespace CanvasCalendar.Services;

/// <summary>
/// Service for reading configuration values from User Secrets or fallback sources
/// </summary>
public interface IConfigurationService
{
    string GetCanvasUrl();
    string GetCanvasApiToken();
    bool HasCanvasCredentials();
}

/// <summary>
/// Configuration service that reads from User Secrets first, then Preferences as fallback
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _configuration;

    public ConfigurationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetCanvasUrl()
    {
        // Try User Secrets first, then fall back to Preferences
        var url = _configuration["Canvas:Url"];
        if (!string.IsNullOrEmpty(url))
            return url;

        return Preferences.Default.Get("Canvas_Url", string.Empty);
    }

    public string GetCanvasApiToken()
    {
        // Try User Secrets first, then fall back to Preferences  
        var token = _configuration["Canvas:ApiToken"];
        if (!string.IsNullOrEmpty(token))
            return token;

        return Preferences.Default.Get("Canvas_ApiToken", string.Empty);
    }

    public bool HasCanvasCredentials()
    {
        var url = GetCanvasUrl();
        var token = GetCanvasApiToken();
        return !string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(token);
    }
}
