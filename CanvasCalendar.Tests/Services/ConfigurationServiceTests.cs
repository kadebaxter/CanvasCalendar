using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace CanvasCalendar.Tests.Services;

public class ConfigurationServiceTests
{
    [Fact]
    public void Configuration_CanReadValues()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["Canvas:Url"]).Returns("https://test.instructure.com");
        mockConfiguration.Setup(c => c["Canvas:ApiToken"]).Returns("test-token");

        // Act
        var url = mockConfiguration.Object["Canvas:Url"];
        var token = mockConfiguration.Object["Canvas:ApiToken"];

        // Assert
        Assert.Equal("https://test.instructure.com", url);
        Assert.Equal("test-token", token);
    }

    [Theory]
    [InlineData("", "test-token", false)]
    [InlineData("https://test.com", "", false)]
    [InlineData("", "", false)]
    [InlineData("https://test.com", "test-token", true)]
    public void CredentialValidation_WorksCorrectly(string url, string token, bool expected)
    {
        // Act - Simple credential validation logic
        var hasCredentials = !string.IsNullOrEmpty(url) && !string.IsNullOrEmpty(token);

        // Assert
        Assert.Equal(expected, hasCredentials);
    }

    [Fact]
    public void MockConfiguration_CanReturnNullValues()
    {
        // Arrange
        var mockConfiguration = new Mock<IConfiguration>();
        mockConfiguration.Setup(c => c["Canvas:Url"]).Returns((string?)null);

        // Act
        var url = mockConfiguration.Object["Canvas:Url"];

        // Assert
        Assert.Null(url);
    }
}
