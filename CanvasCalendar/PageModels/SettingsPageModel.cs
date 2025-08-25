using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CanvasCalendar.Models;
using CanvasCalendar.Services;

namespace CanvasCalendar.PageModels;

public partial class SettingsPageModel : ObservableObject
{
    private readonly ICanvasService _canvasService;
    private readonly IErrorHandler _errorHandler;
    private readonly ILogger<SettingsPageModel> _logger;
    private readonly IConfigurationService _configurationService;

    [ObservableProperty]
    private string _canvasUrl = string.Empty;

    [ObservableProperty]
    private string _canvasApiToken = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _credentialsValid;

    [ObservableProperty]
    private string _validationMessage = string.Empty;

    public SettingsPageModel(
        ICanvasService canvasService,
        IErrorHandler errorHandler,
        ILogger<SettingsPageModel> logger,
        IConfigurationService configurationService)
    {
        _canvasService = canvasService;
        _errorHandler = errorHandler;
        _logger = logger;
        _configurationService = configurationService;
        
        LoadSettings();
    }

    [RelayCommand]
    private async Task ValidateCredentials()
    {
        if (string.IsNullOrWhiteSpace(CanvasUrl) || string.IsNullOrWhiteSpace(CanvasApiToken))
        {
            ValidationMessage = "Please enter both Canvas URL and API Token";
            CredentialsValid = false;
            _logger.LogWarning("Validation attempted with missing credentials. URL: {HasUrl}, Token: {HasToken}", 
                !string.IsNullOrWhiteSpace(CanvasUrl), !string.IsNullOrWhiteSpace(CanvasApiToken));
            return;
        }

        try
        {
            IsBusy = true;
            ValidationMessage = "Validating credentials...";
            
            _logger.LogInformation("Starting Canvas credential validation for URL: {CanvasUrl}", CanvasUrl);
            
            var isValid = await _canvasService.ValidateCredentialsAsync(CanvasUrl, CanvasApiToken);
            
            CredentialsValid = isValid;
            ValidationMessage = isValid 
                ? "✅ Credentials are valid!" 
                : "❌ Invalid credentials. Please check your Canvas URL and API token.";
                
            _logger.LogInformation("Canvas credential validation result: {IsValid}", isValid);
                
            if (isValid)
            {
                await SaveSettings();
            }
        }
        catch (Exception e)
        {
            CredentialsValid = false;
            ValidationMessage = "❌ Error validating credentials. Please check your network connection.";
            _logger.LogError(e, "Error during Canvas credential validation");
            _errorHandler.HandleError(e);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SaveSettings()
    {
        try
        {
            // For now, we'll save to preferences. In a real app, you'd want secure storage.
            Preferences.Default.Set("Canvas_Url", CanvasUrl);
            Preferences.Default.Set("Canvas_ApiToken", CanvasApiToken);
            
            await Shell.Current.DisplayAlert("Settings Saved", 
                "Canvas settings have been saved successfully.", "OK");
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e, "Failed to save settings.");
        }
    }

    [RelayCommand]
    private void ClearSettings()
    {
        CanvasUrl = string.Empty;
        CanvasApiToken = string.Empty;
        CredentialsValid = false;
        ValidationMessage = string.Empty;
        
        Preferences.Default.Remove("Canvas_Url");
        Preferences.Default.Remove("Canvas_ApiToken");
    }

    private void LoadSettings()
    {
        // Load from configuration service (User Secrets first, then preferences)
        var configUrl = _configurationService.GetCanvasUrl();
        var configToken = _configurationService.GetCanvasApiToken();
        
        if (!string.IsNullOrEmpty(configUrl) && !string.IsNullOrEmpty(configToken))
        {
            CanvasUrl = configUrl;
            CanvasApiToken = configToken;
            ValidationMessage = "Credentials loaded from User Secrets";
        }
        else
        {
            // Load from preferences as fallback
            CanvasUrl = Preferences.Default.Get("Canvas_Url", string.Empty);
            CanvasApiToken = Preferences.Default.Get("Canvas_ApiToken", string.Empty);
            
            if (!string.IsNullOrWhiteSpace(CanvasUrl) && !string.IsNullOrWhiteSpace(CanvasApiToken))
            {
                ValidationMessage = "Credentials loaded from Preferences";
            }
        }
    }

    partial void OnCanvasUrlChanged(string value)
    {
        CredentialsValid = false;
        ValidationMessage = string.Empty;
    }

    partial void OnCanvasApiTokenChanged(string value)
    {
        CredentialsValid = false;
        ValidationMessage = string.Empty;
    }
}
