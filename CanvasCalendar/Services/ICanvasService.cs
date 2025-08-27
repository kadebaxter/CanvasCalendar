using CanvasCalendar.Models;

namespace CanvasCalendar.Services;

/// <summary>
/// Canvas LMS API Service Interface
/// </summary>
public interface ICanvasService
{
    Task<bool> ValidateCredentialsAsync(string canvasUrl, string apiToken);
    Task<List<Assignment>> GetUpcomingAssignmentsAsync(string canvasUrl, string apiToken, int days = 7);
    Task<List<Course>> GetCoursesAsync(string canvasUrl, string apiToken);
    Task<List<Assignment>> GetAssignmentsByCourseAsync(string courseId, string canvasUrl, string apiToken);
}
