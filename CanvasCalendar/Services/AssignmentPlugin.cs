using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;
using CanvasCalendar.Data;
using CanvasCalendar.Models;

namespace CanvasCalendar.Services;

/// <summary>
/// Semantic Kernel plugin for assignment-related operations.
/// </summary>
public class AssignmentPlugin
{
    private readonly AssignmentRepository _assignmentRepository;
    private readonly CourseRepository _courseRepository;
    private readonly ICanvasService _canvasService;
    private readonly IConfigurationService _configurationService;

    public AssignmentPlugin(AssignmentRepository assignmentRepository, CourseRepository courseRepository, 
        ICanvasService canvasService, IConfigurationService configurationService)
    {
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
        _canvasService = canvasService;
        _configurationService = configurationService;
    }

    /// <summary>
    /// Gets all assignments for a specific course by course name or code from Canvas API.
    /// </summary>
    /// <param name="courseName">The course name or code to search for</param>
    /// <returns>JSON string containing assignments for the course</returns>
    [KernelFunction("get_assignments_by_course")]
    [Description("Gets all assignments for a specific course by course name or code from Canvas")]
    public async Task<string> GetAssignmentsByCourseAsync(
        [Description("The course name or code (e.g., 'CS3630', 'SE-3630', 'SE3630', 'Computer Science', 'Math 101')")] string courseName)
    {
        try
        {
            // Get Canvas configuration
            var canvasUrl = _configurationService.GetCanvasUrl();
            var apiToken = _configurationService.GetCanvasApiToken();

            if (string.IsNullOrEmpty(canvasUrl) || string.IsNullOrEmpty(apiToken))
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "Canvas configuration not found. Please configure Canvas URL and API token in settings."
                });
            }

            // First, get all courses to find the matching one (efficient single API call)
            var courses = await _canvasService.GetCoursesAsync(canvasUrl, apiToken);
            
            if (!courses.Any())
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    message = "No courses found from Canvas API. Make sure you're connected to Canvas."
                });
            }

            // Find matching course - handle various formats like CS3630, SE-3630-001, etc.
            var normalizedSearchTerm = courseName.Replace("-", "").Replace(" ", "").ToLower();
            var course = courses.FirstOrDefault(c => 
            {
                var normalizedName = c.Name.Replace("-", "").Replace(" ", "").ToLower();
                var normalizedCode = c.Code.Replace("-", "").Replace(" ", "").ToLower();
                
                return normalizedName.Contains(normalizedSearchTerm) ||
                       normalizedCode.Contains(normalizedSearchTerm) ||
                       normalizedSearchTerm.Contains(normalizedCode);
            });

            if (course == null)
            {
                return JsonSerializer.Serialize(new
                {
                    success = false,
                    message = $"No course found matching '{courseName}'. Available courses: {string.Join(", ", courses.Select(c => $"{c.Code} - {c.Name}"))}"
                });
            }

            // Now get assignments for ONLY this specific course (efficient single API call)
            var courseAssignments = await _canvasService.GetAssignmentsByCourseAsync(course.CanvasId, canvasUrl, apiToken);

            if (!courseAssignments.Any())
            {
                return JsonSerializer.Serialize(new
                {
                    success = true,
                    course = new { course.Code, course.Name },
                    message = $"No assignments found for {course.Code} - {course.Name}",
                    assignments = new object[0]
                });
            }

            // Format assignments for the AI
            var formattedAssignments = courseAssignments.Select(a => new
            {
                title = a.Title,
                description = a.Description,
                dueDate = a.DueDate.ToString("yyyy-MM-dd HH:mm"),
                pointsPossible = a.PointsPossible,
                status = a.Status.ToString(),
                isOverdue = a.DueDate < DateTime.Now && a.Status != AssignmentStatus.Completed,
                daysUntilDue = (a.DueDate - DateTime.Now).Days,
                canvasUrl = a.HtmlUrl
            }).ToList();

            return JsonSerializer.Serialize(new
            {
                success = true,
                course = new { course.Code, course.Name },
                totalAssignments = formattedAssignments.Count,
                assignments = formattedAssignments
            }, new JsonSerializerOptions { WriteIndented = true });
        }
        catch (Exception ex)
        {
            return JsonSerializer.Serialize(new
            {
                success = false,
                error = $"Error retrieving assignments from Canvas: {ex.Message}"
            });
        }
    }
}
