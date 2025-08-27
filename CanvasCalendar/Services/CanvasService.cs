using CanvasCalendar.Data;
using CanvasCalendar.Models;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CanvasCalendar.Services;

/// <summary>
/// Canvas LMS API Service Implementation
/// </summary>
public class CanvasService : ICanvasService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public CanvasService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
    }

    public async Task<bool> ValidateCredentialsAsync(string canvasUrl, string apiToken)
    {
        try
        {
            Console.WriteLine($"Validating Canvas credentials for URL: {canvasUrl}");
            
            var normalizedUrl = NormalizeCanvasUrl(canvasUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{normalizedUrl}/api/v1/users/self");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Canvas API validation failed. Status: {response.StatusCode}, Content: {content}");
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error validating Canvas credentials for URL: {canvasUrl}. Error: {ex.Message}");
            return false;
        }
    }

    public async Task<List<Assignment>> GetUpcomingAssignmentsAsync(string canvasUrl, string apiToken, int days = 7)
    {
        try
        {
            var startDate = DateTime.Now.Date;
            var endDate = startDate.AddDays(days);
            
            // First, get all active courses
            var courses = await GetCoursesAsync(canvasUrl, apiToken);
            
            if (!courses.Any())
            {
                Console.WriteLine("No active courses found for assignment fetching");
                return [];
            }
            
            var allAssignments = new List<Assignment>();
            
            // Fetch assignments from each course
            foreach (var course in courses)
            {
                try
                {
                    var courseAssignments = await GetAssignmentsByCourseAsync(course.CanvasId, canvasUrl, apiToken, startDate, endDate);
                    
                    // Ensure course relationship is set
                    foreach (var assignment in courseAssignments)
                    {
                        if (assignment.Course == null)
                        {
                            assignment.Course = course;
                        }
                    }
                    
                    allAssignments.AddRange(courseAssignments);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to fetch assignments for course {course.CanvasId} ({course.Name}): {ex.Message}");
                    // Continue with other courses even if one fails
                }
            }
            
            // Filter assignments to only include those due within the date range
            var filteredAssignments = allAssignments
                .Where(a => a.DueDate >= startDate && a.DueDate <= endDate)
                .OrderBy(a => a.DueDate)
                .ToList();
            
            return filteredAssignments;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching upcoming assignments: {ex.Message}");
            return [];
        }
    }

    private async Task<List<Course>> GetCoursesAsync(string canvasUrl, string apiToken)
    {
        try
        {
            var normalizedUrl = NormalizeCanvasUrl(canvasUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{normalizedUrl}/api/v1/courses?enrollment_state=active&include[]=term");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch courses. Status: {response.StatusCode}");
                return [];
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var canvasCourses = JsonSerializer.Deserialize<List<CanvasCourseDto>>(jsonContent, _jsonOptions);

            return canvasCourses?.Select(MapCanvasCourseToModel).ToList() ?? [];
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching courses from Canvas: {ex.Message}");
            return [];
        }
    }

    private async Task<List<Assignment>> GetAssignmentsByCourseAsync(string courseId, string canvasUrl, string apiToken, DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            var normalizedUrl = NormalizeCanvasUrl(canvasUrl);
            var requestUrl = $"{normalizedUrl}/api/v1/courses/{courseId}/assignments?include[]=course";
            
            // Add date filtering if provided
            if (startDate.HasValue && endDate.HasValue)
            {
                requestUrl += $"&due_after={startDate.Value:yyyy-MM-dd}&due_before={endDate.Value:yyyy-MM-dd}";
            }
            
            var request = new HttpRequestMessage(HttpMethod.Get, requestUrl);
            request.Headers.Add("Authorization", $"Bearer {apiToken}");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Failed to fetch assignments for course {courseId}. Status: {response.StatusCode}");
                return [];
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var canvasAssignments = JsonSerializer.Deserialize<List<CanvasAssignmentDto>>(jsonContent, _jsonOptions);

            var assignments = canvasAssignments?.Select(MapCanvasAssignmentToModel).ToList() ?? [];
            
            return assignments;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching assignments for course {courseId}: {ex.Message}");
            return [];
        }
    }

    private static string NormalizeCanvasUrl(string canvasUrl)
    {
        // Remove trailing slash and ensure proper format
        return canvasUrl.TrimEnd('/');
    }

    private static Course MapCanvasCourseToModel(CanvasCourseDto dto)
    {
        return new Course
        {
            CanvasId = dto.Id.ToString(),
            Name = dto.Name ?? string.Empty,
            Code = dto.CourseCode ?? string.Empty,
            Term = dto.Term?.Name ?? string.Empty,
            AccountId = dto.AccountId?.ToString() ?? string.Empty,
            SisCourseId = dto.SisCourseId ?? string.Empty,
            WorkflowState = dto.WorkflowState ?? string.Empty,
            StartAt = dto.StartAt ?? DateTime.MinValue,
            EndAt = dto.EndAt ?? DateTime.MinValue,
            CreatedAt = dto.CreatedAt ?? DateTime.Now
        };
    }

    private static Assignment MapCanvasAssignmentToModel(CanvasAssignmentDto dto)
    {
        return new Assignment
        {
            CanvasId = dto.Id.ToString(),
            Title = dto.Name ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            DueDate = dto.DueAt ?? DateTime.MaxValue,
            PointsPossible = dto.PointsPossible ?? 0,
            AssignmentGroupId = dto.AssignmentGroupId?.ToString() ?? string.Empty,
            HtmlUrl = dto.HtmlUrl ?? string.Empty,
            Published = dto.Published ?? true,
            CreatedAt = dto.CreatedAt ?? DateTime.Now,
            UpdatedAt = dto.UpdatedAt ?? DateTime.Now,
            Status = AssignmentStatus.New,
            Course = dto.Course != null ? MapCanvasCourseToModel(dto.Course) : null
        };
    }
}

// Canvas API DTOs for deserialization
public class CanvasCourseDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? CourseCode { get; set; }
    public string? WorkflowState { get; set; }
    public int? AccountId { get; set; }
    public string? SisCourseId { get; set; }
    public DateTime? StartAt { get; set; }
    public DateTime? EndAt { get; set; }
    public DateTime? CreatedAt { get; set; }
    public CanvasTermDto? Term { get; set; }
}

public class CanvasTermDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public class CanvasAssignmentDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public DateTime? DueAt { get; set; }
    public double? PointsPossible { get; set; }
    public int? AssignmentGroupId { get; set; }
    public string? HtmlUrl { get; set; }
    public bool? Published { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CourseId { get; set; }
    public CanvasCourseDto? Course { get; set; }
}
