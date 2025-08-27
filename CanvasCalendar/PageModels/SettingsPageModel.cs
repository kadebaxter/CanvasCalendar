using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CanvasCalendar.Models;
using CanvasCalendar.Services;

namespace CanvasCalendar.PageModels;

public partial class SettingsPageModel : ObservableObject
{
    private readonly ISettingsService _settingsService;
    private readonly IDialogService _dialogService;
    private readonly IErrorHandler _errorHandler;

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
        ISettingsService settingsService,
        IDialogService dialogService,
        IErrorHandler errorHandler)
    {
        _settingsService = settingsService;
        _dialogService = dialogService;
        _errorHandler = errorHandler;
        
        LoadSettings();
    }

    [RelayCommand]
    private async Task ValidateCredentials()
    {
        try
        {
            IsBusy = true;
            ValidationMessage = "Validating credentials...";
            
            var result = await _settingsService.ValidateCredentialsAsync(CanvasUrl, CanvasApiToken);
            
            CredentialsValid = result.IsValid;
            ValidationMessage = result.Message;
                
            if (result.IsValid)
            {
                await SaveSettings();
            }
            else if (result.Error != null)
            {
                Console.WriteLine($"Error during Canvas credential validation: {result.Error.Message}");
                _errorHandler.HandleError(result.Error);
            }
        }
        catch (Exception e)
        {
            CredentialsValid = false;
            ValidationMessage = "❌ Unexpected error during validation.";
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
            var success = await _settingsService.SaveSettingsAsync(CanvasUrl, CanvasApiToken);
            
            if (success)
            {
                await _dialogService.ShowAlertAsync("Settings Saved", 
                    "Canvas settings have been saved successfully.");
            }
            else
            {
                await _dialogService.ShowAlertAsync("Save Failed", 
                    "Failed to save settings. Please try again.");
            }
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
        
        _settingsService.ClearSettings();
    }

    private void LoadSettings()
    {
        var settingsData = _settingsService.LoadSettings();
        
        CanvasUrl = settingsData.CanvasUrl;
        CanvasApiToken = settingsData.CanvasApiToken;
        ValidationMessage = settingsData.LoadMessage;
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
