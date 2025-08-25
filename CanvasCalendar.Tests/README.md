# Canvas Assignment Scheduler - Testing

This directory contains unit tests for the Canvas Assignment Scheduler application.

## Test Structure

The test project is organized as follows:

- **BasicTests.cs** - Basic framework tests to ensure xUnit is working correctly
- **Services/CanvasServiceTests.cs** - Tests for HTTP client functionality and basic Canvas API interactions
- **Services/ConfigurationServiceTests.cs** - Tests for configuration and credential validation logic
- **Helpers/TestPreferences.cs** - Mock implementation of MAUI Preferences for testing (currently unused)

## Running Tests

### From Visual Studio
1. Open the solution in Visual Studio
2. Go to Test → Test Explorer
3. Click "Run All Tests"

### From Command Line
```bash
# Run all tests in the solution
dotnet test

# Run tests in the test project only
cd CanvasCalendar.Tests
dotnet test

# Run tests with detailed output
dotnet test --verbosity normal

# Run tests and generate coverage report
dotnet test --collect:"XPlat Code Coverage"
```

## Test Framework

- **xUnit** - Primary testing framework
- **Moq** - Mocking framework for creating test doubles
- **MockHttp** - For mocking HTTP responses in service tests

## Current Test Coverage

### What's Currently Tested
- Basic framework functionality
- HTTP client operations
- JSON serialization/deserialization
- URL normalization logic
- Configuration reading logic
- Basic validation functions

### What's Not Yet Tested (Future Improvements)
- Actual Canvas API integration (requires real credentials)
- Database operations (SQLite repositories)
- MAUI-specific UI components (PageModels, Pages)
- Platform-specific functionality (Preferences, FileSystem)

## Testing Strategy

### Unit Tests vs Integration Tests
- **Unit Tests**: Test individual components in isolation using mocks
- **Integration Tests**: Would test actual Canvas API calls (requires test Canvas instance)

### Mock Strategy
- HTTP calls are mocked using MockHttp to avoid real API calls during testing
- Configuration values are mocked to test different scenarios
- Platform-specific APIs (MAUI Preferences) are abstracted where possible

## Adding New Tests

### Testing a Service
1. Create a new test file in the appropriate folder (e.g., `Services/NewServiceTests.cs`)
2. Use Moq to mock dependencies
3. Test both success and failure scenarios
4. Include edge cases and validation

Example:
```csharp
public class NewServiceTests
{
    [Fact]
    public void NewService_DoesExpectedThing()
    {
        // Arrange
        var mockDependency = new Mock<IDependency>();
        var service = new NewService(mockDependency.Object);
        
        // Act
        var result = service.DoSomething();
        
        // Assert
        Assert.True(result);
    }
}
```

### Testing Canvas Integration
For testing actual Canvas API calls:
1. Create a separate integration test project
2. Use real Canvas test instance credentials
3. Mark tests with `[Fact(Skip = "Integration test")]` for CI/CD

## Limitations

Due to MAUI's platform-specific nature, some components are difficult to unit test:
- PageModels that depend on Shell navigation
- Repository classes that use platform-specific file paths
- Services that depend on MAUI Essentials

For these components, consider:
- Creating testable abstractions
- Using integration tests instead
- Testing the business logic separately from the UI logic

## Future Improvements

1. **Add Repository Tests**: Create in-memory SQLite tests for data access
2. **Add PageModel Tests**: Create testable abstractions for MAUI-specific functionality
3. **Add Integration Tests**: Test actual Canvas API interactions
4. **Add Performance Tests**: Test large data set handling
5. **Add UI Tests**: Consider MAUI UI testing frameworks for end-to-end testing
