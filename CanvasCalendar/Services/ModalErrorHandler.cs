namespace CanvasCalendar.Services;

/// <summary>
/// Modal Error Handler Service Implementation.
/// </summary>
public class ModalErrorHandler : IErrorHandler
{
    public ModalErrorHandler()
    {
    }

    public void HandleError(Exception ex)
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        
        // Show user-friendly error message
        var userMessage = GetUserFriendlyMessage(ex);
        ShowErrorToUser(userMessage);
    }

    public void HandleError(Exception ex, string userMessage)
    {
        Console.WriteLine($"An error occurred: {userMessage}. Details: {ex.Message}");
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
