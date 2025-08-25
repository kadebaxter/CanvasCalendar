namespace CanvasCalendar.Data;

public static class Constants
{
    public const string DatabaseFilename = "CanvasCalendarSQLite.db3";

    public static string DatabasePath =>
        $"Data Source={Path.Combine(FileSystem.AppDataDirectory, DatabaseFilename)}";

    // API Configuration
    public const int CanvasApiRateLimit = 1000; // requests per hour
    public const int DefaultSessionMinutes = 60;
    public const int MinSessionMinutes = 15;
    public const int MaxSessionMinutes = 240;
    
    // Canvas API Endpoints
    public const string CanvasApiVersion = "v1";
    public const string AssignmentsEndpoint = "/api/v1/users/self/assignments";
    public const string CoursesEndpoint = "/api/v1/courses";
    
    // Date/Time Constants
    public const int DaysToFetchAssignments = 7; // Current week
    
    // Google Calendar Settings (for future use)
    public const string GoogleCalendarScope = "https://www.googleapis.com/auth/calendar.events";
    public const string ApplicationName = "Canvas Assignment Scheduler";
}
