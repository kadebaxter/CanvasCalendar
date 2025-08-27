using CanvasCalendar.Data;
using CanvasCalendar.Models;

namespace CanvasCalendar.Services;

/// <summary>
/// Service for synchronizing assignments and courses from Canvas
/// </summary>
public class AssignmentSyncService : IAssignmentSyncService
{
    private readonly AssignmentRepository _assignmentRepository;
    private readonly CourseRepository _courseRepository;
    private readonly ICanvasService _canvasService;
    private readonly IConfigurationService _configurationService;

    public AssignmentSyncService(
        AssignmentRepository assignmentRepository,
        CourseRepository courseRepository,
        ICanvasService canvasService,
        IConfigurationService configurationService)
    {
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
        _canvasService = canvasService;
        _configurationService = configurationService;
    }

    public async Task<SyncResult> SyncWithCanvasAsync()
    {
        try
        {
            var validationResult = ValidateCredentials();
            if (!validationResult.Success)
                return validationResult;

            var apiToken = _configurationService.GetCanvasApiToken();
            var canvasUrl = _configurationService.GetCanvasUrl();

            // Get all upcoming assignments (this internally gets courses and assignments)
            var canvasAssignments = await _canvasService.GetUpcomingAssignmentsAsync(canvasUrl, apiToken);
            
            if (!canvasAssignments.Any())
            {
                return new SyncResult
                {
                    Success = true,
                    SyncedAssignments = 0,
                    SyncedCourses = 0,
                    Message = "No upcoming assignments found for the next 7 days."
                };
            }

            // Sync courses and assignments
            var syncedCourses = await SyncCoursesAsync(canvasAssignments);
            var syncedAssignments = await SyncAssignmentsAsync(canvasAssignments);

            return new SyncResult
            {
                Success = true,
                SyncedAssignments = syncedAssignments,
                SyncedCourses = syncedCourses,
                Message = $"Synchronized {syncedAssignments} new assignments from Canvas."
            };
        }
        catch (Exception ex)
        {
            return new SyncResult
            {
                Success = false,
                Error = ex,
                Message = "Failed to sync with Canvas. Please check your credentials and try again."
            };
        }
    }

    private SyncResult ValidateCredentials()
    {
        if (!_configurationService.HasCanvasCredentials())
        {
            return new SyncResult
            {
                Success = false,
                Message = "Configuration Required: Please configure your Canvas settings first."
            };
        }

        var apiToken = _configurationService.GetCanvasApiToken();
        if (string.IsNullOrEmpty(apiToken))
        {
            return new SyncResult
            {
                Success = false,
                Message = "Canvas API token not found. Please configure it in Settings."
            };
        }

        return new SyncResult { Success = true };
    }

    private async Task<int> SyncCoursesAsync(List<Assignment> canvasAssignments)
    {
        var uniqueCourses = canvasAssignments
            .Where(a => a.Course != null)
            .Select(a => a.Course!)
            .GroupBy(c => c.CanvasId)
            .Select(g => g.First())
            .ToList();

        var syncedCount = 0;
        foreach (var course in uniqueCourses)
        {
            var existingCourse = await _courseRepository.GetByCanvasIdAsync(course.CanvasId);
            if (existingCourse == null)
            {
                await _courseRepository.SaveItemAsync(course);
                syncedCount++;
            }
        }

        return syncedCount;
    }

    private async Task<int> SyncAssignmentsAsync(List<Assignment> canvasAssignments)
    {
        var syncedCount = 0;
        foreach (var assignment in canvasAssignments)
        {
            var existingAssignment = await _assignmentRepository.GetByCanvasIdAsync(assignment.CanvasId);
            
            if (assignment.Course != null)
            {
                var course = await _courseRepository.GetByCanvasIdAsync(assignment.Course.CanvasId);
                if (course != null)
                {
                    assignment.CourseID = course.ID;
                    assignment.Course = course;
                }
            }

            if (existingAssignment == null)
            {
                await _assignmentRepository.SaveItemAsync(assignment);
                syncedCount++;
            }
            else
            {
                // Update existing assignment
                UpdateExistingAssignment(existingAssignment, assignment);
                await _assignmentRepository.SaveItemAsync(existingAssignment);
            }
        }

        return syncedCount;
    }

    private static void UpdateExistingAssignment(Assignment existing, Assignment updated)
    {
        existing.Title = updated.Title;
        existing.Description = updated.Description;
        existing.DueDate = updated.DueDate;
        existing.PointsPossible = updated.PointsPossible;
        existing.UpdatedAt = DateTime.Now;
    }
}
