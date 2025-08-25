using Microsoft.Extensions.Logging;

namespace CanvasCalendar.Services;

/// <summary>
/// Modal Error Handler Service Implementation.
/// </summary>
public class ModalErrorHandler : IErrorHandler
{
    private readonly ILogger<ModalErrorHandler> _logger;

    public ModalErrorHandler(ILogger<ModalErrorHandler> logger)
    {
        _logger = logger;
    }

    public void HandleError(Exception ex)
    {
        _logger.LogError(ex, "An error occurred");
        
        // Show user-friendly error message
        var userMessage = GetUserFriendlyMessage(ex);
        ShowErrorToUser(userMessage);
    }

    public void HandleError(Exception ex, string userMessage)
    {
        _logger.LogError(ex, "An error occurred: {UserMessage}", userMessage);
        ShowErrorToUser(userMessage);
    }

    private static string GetUserFriendlyMessage(Exception ex)
    {
        return ex switch
        {
            HttpRequestException => "Network error. Please check your internet connection and try again.",
            UnauthorizedAccessException => "Invalid Canvas credentials. Please check your Canvas URL and API token.",
            TimeoutException => "Request timed out. Please try again.",
            _ => "An unexpected error occurred. Please try again."
        };
    }

    private static void ShowErrorToUser(string message)
    {
        // For now, we'll use the main thread dispatcher to show alerts
        // This could be enhanced with more sophisticated error UI later
        if (Application.Current?.Windows?.FirstOrDefault()?.Page != null)
        {
            var mainPage = Application.Current.Windows.FirstOrDefault()?.Page;
            mainPage?.Dispatcher.Dispatch(async () =>
            {
                await mainPage.DisplayAlert("Error", message, "OK");
            });
        }
    }
}
