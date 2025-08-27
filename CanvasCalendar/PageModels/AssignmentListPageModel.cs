using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CanvasCalendar.Data;
using CanvasCalendar.Models;
using CanvasCalendar.Services;

namespace CanvasCalendar.PageModels;

public partial class AssignmentListPageModel : ObservableObject
{
    private readonly AssignmentRepository _assignmentRepository;
    private readonly IAssignmentSyncService _assignmentSyncService;
    private readonly IDialogService _dialogService;
    private readonly INavigationService _navigationService;
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
        IAssignmentSyncService assignmentSyncService,
        IDialogService dialogService,
        INavigationService navigationService,
        IErrorHandler errorHandler,
        IConfigurationService configurationService)
    {
        _assignmentRepository = assignmentRepository;
        _assignmentSyncService = assignmentSyncService;
        _dialogService = dialogService;
        _navigationService = navigationService;
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
            await _dialogService.ShowAlertAsync("Configuration Required", 
                "Please configure your Canvas settings first.");
            await _navigationService.NavigateToSettingsAsync();
            return;
        }

        try
        {
            IsBusy = true;
            
            var result = await _assignmentSyncService.SyncWithCanvasAsync();
            
            if (result.Success)
            {
                await _dialogService.ShowAlertAsync("Sync Complete", result.Message);
                await LoadAssignments();
            }
            else
            {
                if (result.Error != null)
                {
                    _errorHandler.HandleError(result.Error, result.Message);
                }
                else
                {
                    await _dialogService.ShowAlertAsync("Sync Failed", result.Message);
                }
            }
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
        => _navigationService.NavigateToAssignmentAsync(assignment.ID);

    [RelayCommand]
    private Task NavigateToSettings()
        => _navigationService.NavigateToSettingsAsync();

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
