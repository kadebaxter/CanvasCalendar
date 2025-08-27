using Microsoft.Extensions.Configuration;

namespace CanvasCalendar.Services;

/// <summary>
/// Service for reading and writing configuration values from User Secrets or fallback sources
/// </summary>
public interface IConfigurationService
{
    string GetCanvasUrl();
    string GetCanvasApiToken();
    bool HasCanvasCredentials();
    Task SaveCanvasSettingsAsync(string url, string apiToken);
    void ClearCanvasSettings();
    ConfigurationSource GetConfigurationSource();
}

/// <summary>
/// Indicates the source of configuration values
/// </summary>
public enum ConfigurationSource
{
    None,
    UserSecrets,
    Preferences
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

    public async Task SaveCanvasSettingsAsync(string url, string apiToken)
    {
        // For now, we'll save to preferences. In a real app, you'd want secure storage.
        Preferences.Default.Set("Canvas_Url", url);
        Preferences.Default.Set("Canvas_ApiToken", apiToken);
        await Task.CompletedTask;
    }

    public void ClearCanvasSettings()
    {
        Preferences.Default.Remove("Canvas_Url");
        Preferences.Default.Remove("Canvas_ApiToken");
    }

    public ConfigurationSource GetConfigurationSource()
    {
        // Check if we have values in User Secrets
        var userSecretsUrl = _configuration["Canvas:Url"];
        var userSecretsToken = _configuration["Canvas:ApiToken"];
        
        if (!string.IsNullOrEmpty(userSecretsUrl) && !string.IsNullOrEmpty(userSecretsToken))
            return ConfigurationSource.UserSecrets;

        // Check if we have values in Preferences
        var prefsUrl = Preferences.Default.Get("Canvas_Url", string.Empty);
        var prefsToken = Preferences.Default.Get("Canvas_ApiToken", string.Empty);
        
        if (!string.IsNullOrEmpty(prefsUrl) && !string.IsNullOrEmpty(prefsToken))
            return ConfigurationSource.Preferences;

        return ConfigurationSource.None;
    }
}
