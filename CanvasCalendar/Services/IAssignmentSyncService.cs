using CanvasCalendar.Models;

namespace CanvasCalendar.Services;

/// <summary>
/// Service for synchronizing assignments and courses from Canvas
/// </summary>
public interface IAssignmentSyncService
{
    Task<SyncResult> SyncWithCanvasAsync();
}

/// <summary>
/// Result of a synchronization operation
/// </summary>
public class SyncResult
{
    public bool Success { get; set; }
    public int SyncedAssignments { get; set; }
    public int SyncedCourses { get; set; }
    public string Message { get; set; } = string.Empty;
    public Exception? Error { get; set; }
}
