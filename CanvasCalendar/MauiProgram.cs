using CommunityToolkit.Maui;
using Microsoft.Extensions.Configuration;
using CanvasCalendar.Data;
using CanvasCalendar.Services;
using CanvasCalendar.PageModels;
using CanvasCalendar.Pages;

namespace CanvasCalendar;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Add User Secrets for Canvas configuration
#if DEBUG
        builder.Configuration.AddUserSecrets<App>();
#endif

        // Add HTTP client
        builder.Services.AddHttpClient();

        // Configuration Services
        builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();

        // Repository Services
        builder.Services.AddSingleton<CourseRepository>();
        builder.Services.AddSingleton<AssignmentRepository>();

        // Business Services
        builder.Services.AddSingleton<ICanvasService, CanvasService>();
        builder.Services.AddSingleton<IErrorHandler, ModalErrorHandler>();
        builder.Services.AddSingleton<IAssignmentSyncService, AssignmentSyncService>();
        builder.Services.AddSingleton<IDialogService, DialogService>();
        builder.Services.AddSingleton<INavigationService, NavigationService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();

        // Page Models
        builder.Services.AddSingleton<AssignmentListPageModel>();
        builder.Services.AddSingleton<SettingsPageModel>();

        // Pages
        builder.Services.AddSingleton<AssignmentListPage>();
        builder.Services.AddSingleton<SettingsPage>();

        return builder.Build();
    }
}
