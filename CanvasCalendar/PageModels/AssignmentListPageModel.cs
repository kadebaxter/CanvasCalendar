using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CanvasCalendar.Data;
using CanvasCalendar.Models;
using CanvasCalendar.Services;

namespace CanvasCalendar.PageModels;

public partial class AssignmentListPageModel : ObservableObject
{
    private readonly AssignmentRepository _assignmentRepository;
    private readonly CourseRepository _courseRepository;
    private readonly ICanvasService _canvasService;
    private readonly IErrorHandler _errorHandler;
    private readonly IConfigurationService _configurationService;

    [ObservableProperty]
    private List<Assignment> _assignments = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _isRefreshing;

    [ObservableProperty]
    private string _canvasUrl = string.Empty;

    [ObservableProperty]
    private bool _hasCredentials;

    public AssignmentListPageModel(
        AssignmentRepository assignmentRepository,
        CourseRepository courseRepository,
        ICanvasService canvasService,
        IErrorHandler errorHandler,
        IConfigurationService configurationService)
    {
        _assignmentRepository = assignmentRepository;
        _courseRepository = courseRepository;
        _canvasService = canvasService;
        _errorHandler = errorHandler;
        _configurationService = configurationService;
        
        CheckCredentials();
    }

    [RelayCommand]
    private async Task Refresh()
    {
        try
        {
            IsRefreshing = true;
            await LoadAssignments();
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
        finally
        {
            IsRefreshing = false;
        }
    }

    [RelayCommand]
    private async Task SyncWithCanvas()
    {
        if (!HasCredentials)
        {
            await Shell.Current.DisplayAlert("Configuration Required", 
                "Please configure your Canvas settings first.", "OK");
            await Shell.Current.GoToAsync("settings");
            return;
        }

        try
        {
            IsBusy = true;
            
            var apiToken = _configurationService.GetCanvasApiToken();
            if (string.IsNullOrEmpty(apiToken))
            {
                _errorHandler.HandleError(new InvalidOperationException("Canvas API token not configured"),
                    "Canvas API token not found. Please configure it in Settings.");
                return;
            }

            // Fetch upcoming assignments from Canvas
            var canvasAssignments = await _canvasService.GetUpcomingAssignmentsAsync(CanvasUrl, apiToken);
            
            if (!canvasAssignments.Any())
            {
                await Shell.Current.DisplayAlert("No Assignments", 
                    "No upcoming assignments found for the next 7 days.", "OK");
                return;
            }

            // Sync courses first
            var uniqueCourses = canvasAssignments
                .Where(a => a.Course != null)
                .Select(a => a.Course!)
                .GroupBy(c => c.CanvasId)
                .Select(g => g.First())
                .ToList();

            foreach (var course in uniqueCourses)
            {
                var existingCourse = await _courseRepository.GetByCanvasIdAsync(course.CanvasId);
                if (existingCourse == null)
                {
                    await _courseRepository.SaveItemAsync(course);
                }
            }

            // Sync assignments
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
                    existingAssignment.Title = assignment.Title;
                    existingAssignment.Description = assignment.Description;
                    existingAssignment.DueDate = assignment.DueDate;
                    existingAssignment.PointsPossible = assignment.PointsPossible;
                    existingAssignment.UpdatedAt = DateTime.Now;
                    
                    await _assignmentRepository.SaveItemAsync(existingAssignment);
                }
            }

            await Shell.Current.DisplayAlert("Sync Complete", 
                $"Synchronized {syncedCount} new assignments from Canvas.", "OK");
            
            await LoadAssignments();
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e, "Failed to sync with Canvas. Please check your credentials and try again.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task NavigateToAssignment(Assignment assignment)
        => Shell.Current.GoToAsync($"assignment?id={assignment.ID}");

    [RelayCommand]
    private Task NavigateToSettings()
        => Shell.Current.GoToAsync("settings");

    private async Task LoadAssignments()
    {
        try
        {
            IsBusy = true;
            
            // Load assignments due in the next 7 days
            var startDate = DateTime.Now.Date;
            var endDate = startDate.AddDays(Constants.DaysToFetchAssignments);
            
            var assignments = await _assignmentRepository.GetAssignmentsDueInRangeAsync(startDate, endDate);
            Assignments = assignments;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void CheckCredentials()
    {
        HasCredentials = _configurationService.HasCanvasCredentials();
        CanvasUrl = _configurationService.GetCanvasUrl();
    }

    [RelayCommand]
    private async Task Appearing()
    {
        CheckCredentials();
        await LoadAssignments();
    }
}
