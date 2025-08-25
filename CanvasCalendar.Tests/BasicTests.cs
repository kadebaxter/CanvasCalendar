using Xunit;

namespace CanvasCalendar.Tests;

/// <summary>
/// Basic tests to ensure the test framework is working
/// </summary>
public class BasicTests
{
    [Fact]
    public void BasicMath_WorksCorrectly()
    {
        // Arrange
        var a = 2;
        var b = 3;
        
        // Act
        var result = a + b;
        
        // Assert
        Assert.Equal(5, result);
    }

    [Theory]
    [InlineData(1, 2, 3)]
    [InlineData(0, 0, 0)]
    [InlineData(-1, 1, 0)]
    public void Addition_WithDifferentInputs_ReturnsCorrectSum(int a, int b, int expected)
    {
        // Act
        var result = a + b;
        
        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void StringManipulation_WorksCorrectly()
    {
        // Arrange
        var input = "Canvas Assignment Scheduler";
        
        // Act
        var result = input.ToUpper();
        
        // Assert
        Assert.Equal("CANVAS ASSIGNMENT SCHEDULER", result);
        Assert.Contains("CANVAS", result);
    }

    [Fact]
    public void DateTimeOperations_WorkCorrectly()
    {
        // Arrange
        var baseDate = new DateTime(2025, 8, 25);
        
        // Act
        var futureDate = baseDate.AddDays(7);
        var pastDate = baseDate.AddDays(-1);
        
        // Assert
        Assert.True(futureDate > baseDate);
        Assert.True(pastDate < baseDate);
        Assert.Equal(7, (futureDate - baseDate).Days);
    }

    [Fact]
    public void Collections_WorkCorrectly()
    {
        // Arrange
        var assignments = new List<string> { "Assignment 1", "Assignment 2", "Assignment 3" };
        
        // Act
        var filteredAssignments = assignments.Where(a => a.Contains("1")).ToList();
        
        // Assert
        Assert.Single(filteredAssignments);
        Assert.Equal("Assignment 1", filteredAssignments.First());
    }
}
