using CanvasCalendar.Services;

namespace CanvasCalendar.Services;

/// <summary>
/// Service for managing settings and credential validation
/// </summary>
public interface ISettingsService
{
    Task<SettingsValidationResult> ValidateCredentialsAsync(string canvasUrl, string canvasApiToken);
    Task<bool> SaveSettingsAsync(string canvasUrl, string canvasApiToken);
    void ClearSettings();
    SettingsData LoadSettings();
}

/// <summary>
/// Result of credential validation
/// </summary>
public class SettingsValidationResult
{
    public bool IsValid { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Error { get; set; }
}

/// <summary>
/// Settings data container
/// </summary>
public class SettingsData
{
    public string CanvasUrl { get; set; } = string.Empty;
    public string CanvasApiToken { get; set; } = string.Empty;
    public string LoadMessage { get; set; } = string.Empty;
    public ConfigurationSource Source { get; set; }
}

/// <summary>
/// Implementation of settings service
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly ICanvasService _canvasService;
    private readonly IConfigurationService _configurationService;

    public SettingsService(ICanvasService canvasService, IConfigurationService configurationService)
    {
        _canvasService = canvasService;
        _configurationService = configurationService;
    }

    public async Task<SettingsValidationResult> ValidateCredentialsAsync(string canvasUrl, string canvasApiToken)
    {
        if (string.IsNullOrWhiteSpace(canvasUrl) || string.IsNullOrWhiteSpace(canvasApiToken))
        {
            return new SettingsValidationResult
            {
                IsValid = false,
                Message = "Please enter both Canvas URL and API Token"
            };
        }

        try
        {
            var isValid = await _canvasService.ValidateCredentialsAsync(canvasUrl, canvasApiToken);
            
            return new SettingsValidationResult
            {
                IsValid = isValid,
                Message = isValid 
                    ? "✅ Credentials are valid!" 
                    : "❌ Invalid credentials. Please check your Canvas URL and API token."
            };
        }
        catch (Exception ex)
        {
            return new SettingsValidationResult
            {
                IsValid = false,
                Message = "❌ Error validating credentials. Please check your network connection.",
                Error = ex
            };
        }
    }

    public async Task<bool> SaveSettingsAsync(string canvasUrl, string canvasApiToken)
    {
        try
        {
            await _configurationService.SaveCanvasSettingsAsync(canvasUrl, canvasApiToken);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public void ClearSettings()
    {
        _configurationService.ClearCanvasSettings();
    }

    public SettingsData LoadSettings()
    {
        var source = _configurationService.GetConfigurationSource();
        var url = _configurationService.GetCanvasUrl();
        var token = _configurationService.GetCanvasApiToken();

        var loadMessage = source switch
        {
            ConfigurationSource.UserSecrets => "Credentials loaded from User Secrets",
            ConfigurationSource.Preferences => "Credentials loaded from Preferences",
            _ => string.Empty
        };

        return new SettingsData
        {
            CanvasUrl = url,
            CanvasApiToken = token,
            LoadMessage = loadMessage,
            Source = source
        };
    }
}
