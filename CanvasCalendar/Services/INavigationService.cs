namespace CanvasCalendar.Services;

/// <summary>
/// Service for handling navigation between pages
/// </summary>
public interface INavigationService
{
    Task NavigateToAsync(string route);
    Task NavigateToAssignmentAsync(int assignmentId);
    Task NavigateToSettingsAsync();
}

/// <summary>
/// Implementation of navigation service using Shell.Current.GoToAsync
/// </summary>
public class NavigationService : INavigationService
{
    public Task NavigateToAsync(string route)
    {
        return Shell.Current.GoToAsync(route);
    }

    public Task NavigateToAssignmentAsync(int assignmentId)
    {
        return Shell.Current.GoToAsync($"assignment?id={assignmentId}");
    }

    public Task NavigateToSettingsAsync()
    {
        return Shell.Current.GoToAsync("settings");
    }
}
