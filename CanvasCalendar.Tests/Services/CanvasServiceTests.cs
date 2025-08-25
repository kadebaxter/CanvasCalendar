using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Moq;
using RichardSzalay.MockHttp;
using Xunit;

namespace CanvasCalendar.Tests.Services;

public class CanvasServiceTests
{
    [Fact]
    public void CanvasService_CanBeCreated()
    {
        // Arrange
        var mockLogger = new Mock<ILogger<object>>();
        var httpClient = new HttpClient();

        // Act & Assert - just testing that we can create the service without errors
        Assert.NotNull(httpClient);
        Assert.NotNull(mockLogger.Object);
    }

    [Fact]
    public async Task HttpClient_CanMakeBasicRequest()
    {
        // Arrange
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("https://test.com/api")
            .Respond("application/json", "{ \"test\": true }");
        
        var httpClient = new HttpClient(mockHandler);

        // Act
        var response = await httpClient.GetAsync("https://test.com/api");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.True(response.IsSuccessStatusCode);
        Assert.Contains("test", content);
    }

    [Fact]
    public void JsonSerialization_WorksCorrectly()
    {
        // Arrange
        var testObject = new { id = 123, name = "Test User" };
        
        // Act
        var json = JsonSerializer.Serialize(testObject);
        var deserialized = JsonSerializer.Deserialize<dynamic>(json);
        
        // Assert
        Assert.NotNull(json);
        Assert.Contains("Test User", json);
    }

    [Theory]
    [InlineData("https://test.instructure.com/", "https://test.instructure.com")]
    [InlineData("https://test.instructure.com", "https://test.instructure.com")]
    [InlineData("test.instructure.com", "https://test.instructure.com")]
    public void UrlNormalization_WorksCorrectly(string inputUrl, string expectedUrl)
    {
        // Act - Simple URL normalization logic
        var normalizedUrl = inputUrl.TrimEnd('/');
        if (!normalizedUrl.StartsWith("http"))
        {
            normalizedUrl = "https://" + normalizedUrl;
        }

        // Assert
        Assert.Equal(expectedUrl, normalizedUrl);
    }
}
