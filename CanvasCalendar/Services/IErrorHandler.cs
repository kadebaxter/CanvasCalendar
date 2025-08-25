namespace CanvasCalendar.Services;

/// <summary>
/// Error Handler Service Interface.
/// </summary>
public interface IErrorHandler
{
    /// <summary>
    /// Handle error in UI.
    /// </summary>
    /// <param name="ex">Exception being thrown.</param>
    void HandleError(Exception ex);
    
    /// <summary>
    /// Handle error with custom message.
    /// </summary>
    /// <param name="ex">Exception being thrown.</param>
    /// <param name="userMessage">User-friendly message to display.</param>
    void HandleError(Exception ex, string userMessage);
}
