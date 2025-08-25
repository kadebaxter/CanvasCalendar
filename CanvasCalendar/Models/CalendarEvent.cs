using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCalendar.Models;

public partial class CalendarEvent : ObservableObject
{
    public int ID { get; set; }
    public string GoogleEventId { get; set; } = string.Empty;
    public int AssignmentID { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public EventStatus Status { get; set; } = EventStatus.Scheduled;
}

public enum EventStatus
{
    Scheduled,
    InProgress,
    Completed,
    Cancelled
}
