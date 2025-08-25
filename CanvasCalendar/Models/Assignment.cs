using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCalendar.Models;

public partial class Assignment : ObservableObject
{
    public int ID { get; set; }
    public string CanvasId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime DueDate { get; set; }
    
    [JsonIgnore]
    public int CourseID { get; set; }
    
    public Course? Course { get; set; }
    public double PointsPossible { get; set; }
    public TimeEstimate? EstimatedTime { get; set; }
    public List<CalendarEvent> ScheduledSessions { get; set; } = [];
    public AssignmentStatus Status { get; set; } = AssignmentStatus.New;
    
    // Canvas API specific fields
    public string AssignmentGroupId { get; set; } = string.Empty;
    public string HtmlUrl { get; set; } = string.Empty;
    public bool Published { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    public override string ToString() => $"{Title}";
}

public enum AssignmentStatus
{
    New,
    Scheduled,
    InProgress,
    Completed,
    Overdue
}
