using CanvasCalendar.Data;
using CanvasCalendar.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CanvasCalendar.Services;

/// <summary>
/// Canvas LMS API Service Implementation
/// </summary>
public class CanvasService : ICanvasService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CanvasService> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CanvasService(HttpClient httpClient, ILogger<CanvasService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        
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
            _logger.LogInformation("Validating Canvas credentials for URL: {CanvasUrl}", canvasUrl);
            
            var normalizedUrl = NormalizeCanvasUrl(canvasUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, $"{normalizedUrl}/api/v1/users/self");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");

            _logger.LogDebug("Making request to: {RequestUrl}", request.RequestUri);
            
            var response = await _httpClient.SendAsync(request);
            
            _logger.LogInformation("Canvas API response: {StatusCode}", response.StatusCode);
            
            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Canvas API validation failed. Status: {StatusCode}, Content: {Content}", 
                    response.StatusCode, content);
            }
            
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating Canvas credentials for URL: {CanvasUrl}", canvasUrl);
            return false;
        }
    }

    public async Task<List<Course>> GetCoursesAsync(string canvasUrl, string apiToken)
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
                _logger.LogError("Failed to fetch courses. Status: {StatusCode}", response.StatusCode);
                return [];
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var canvasCourses = JsonSerializer.Deserialize<List<CanvasCourseDto>>(jsonContent, _jsonOptions);

            return canvasCourses?.Select(MapCanvasCourseToModel).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching courses from Canvas");
            return [];
        }
    }

    public async Task<List<Assignment>> GetAssignmentsAsync(string canvasUrl, string apiToken)
    {
        try
        {
            var normalizedUrl = NormalizeCanvasUrl(canvasUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{normalizedUrl}/api/v1/users/self/assignments?include[]=course");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch assignments. Status: {StatusCode}", response.StatusCode);
                return [];
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var canvasAssignments = JsonSerializer.Deserialize<List<CanvasAssignmentDto>>(jsonContent, _jsonOptions);

            return canvasAssignments?.Select(MapCanvasAssignmentToModel).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching assignments from Canvas");
            return [];
        }
    }

    public async Task<List<Assignment>> GetUpcomingAssignmentsAsync(string canvasUrl, string apiToken, int days = 7)
    {
        try
        {
            var startDate = DateTime.Now;
            var endDate = startDate.AddDays(days);
            
            var normalizedUrl = NormalizeCanvasUrl(canvasUrl);
            var request = new HttpRequestMessage(HttpMethod.Get, 
                $"{normalizedUrl}/api/v1/users/self/assignments" +
                $"?bucket=upcoming&include[]=course" +
                $"&due_after={startDate:yyyy-MM-dd}" +
                $"&due_before={endDate:yyyy-MM-dd}");
            request.Headers.Add("Authorization", $"Bearer {apiToken}");

            var response = await _httpClient.SendAsync(request);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Failed to fetch upcoming assignments. Status: {StatusCode}", response.StatusCode);
                return [];
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            var canvasAssignments = JsonSerializer.Deserialize<List<CanvasAssignmentDto>>(jsonContent, _jsonOptions);

            return canvasAssignments?.Select(MapCanvasAssignmentToModel).ToList() ?? [];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching upcoming assignments from Canvas");
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
