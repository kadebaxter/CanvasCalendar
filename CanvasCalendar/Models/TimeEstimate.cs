using CommunityToolkit.Mvvm.ComponentModel;

namespace CanvasCalendar.Models;

public partial class TimeEstimate : ObservableObject
{
    public int ID { get; set; }
    public int AssignmentID { get; set; }
    public double HoursEstimated { get; set; }
    public double ConfidenceLevel { get; set; }
    public bool IsUserModified { get; set; }
    public string LLMReasoning { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
