using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCalendar.Models;

public partial class UserPreferences : ObservableObject
{
    public int ID { get; set; }
    public TimeOnly PreferredStartTime { get; set; } = new(9, 0);
    public TimeOnly PreferredEndTime { get; set; } = new(18, 0);
    public int MinimumSessionMinutes { get; set; } = 30;
    public int MaximumSessionMinutes { get; set; } = 180;
    public int BreakTimeMinutes { get; set; } = 15;
    public string SelectedCalendarId { get; set; } = string.Empty;
    public bool AutoScheduleEnabled { get; set; } = true;
    public string CanvasUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
