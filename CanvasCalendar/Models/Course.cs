using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCalendar.Models;

public partial class Course : ObservableObject
{
    public int ID { get; set; }
    public string CanvasId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string Term { get; set; } = string.Empty;
    public List<Assignment> Assignments { get; set; } = [];
    
    // Canvas API specific fields
    public string AccountId { get; set; } = string.Empty;
    public string SisCourseId { get; set; } = string.Empty;
    public string WorkflowState { get; set; } = string.Empty;
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public override string ToString() => $"{Code} - {Name}";
}
