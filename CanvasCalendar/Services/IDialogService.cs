namespace CanvasCalendar.Services;

/// <summary>
/// Service for handling user dialogs and alerts
/// </summary>
public interface IDialogService
{
    Task ShowAlertAsync(string title, string message, string cancel = "OK");
    Task<bool> ShowConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No");
}

/// <summary>
/// Implementation of dialog service using Shell.Current.DisplayAlert
/// </summary>
public class DialogService : IDialogService
{
    public Task ShowAlertAsync(string title, string message, string cancel = "OK")
    {
        return Shell.Current.DisplayAlert(title, message, cancel);
    }

    public Task<bool> ShowConfirmAsync(string title, string message, string accept = "Yes", string cancel = "No")
    {
        return Shell.Current.DisplayAlert(title, message, accept, cancel);
    }
}
