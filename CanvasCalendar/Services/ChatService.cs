using Microsoft.Extensions.AI;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using CanvasCalendar.Data;

namespace CanvasCalendar.Services;

/// <summary>
/// Service implementation for AI chat functionality using Semantic Kernel with Ollama.
/// </summary>
public class ChatService : IChatService
{
    private readonly Kernel _kernel;
    private readonly ChatHistory _skChatHistory;
    private readonly List<ChatMessage> _chatHistory;

    public ChatService(AssignmentRepository assignmentRepository, CourseRepository courseRepository, 
        ICanvasService canvasService, IConfigurationService configurationService)
    {
        // Create kernel builder
        var builder = Kernel.CreateBuilder();
        
        // Add OpenAI-compatible chat completion service (Ollama is OpenAI-compatible)
        builder.AddOpenAIChatCompletion(
            modelId: "llama3.2:3b", // Smaller model for better CPU performance
            endpoint: new Uri("http://localhost:11434/v1/"),
            apiKey: "not-needed" // Ollama doesn't require API key
        );
        
        // Add simple calculator plugin for testing
        builder.Plugins.AddFromObject(new CalculatorPlugin());
        
        // Add our assignment plugin (temporarily disabled for testing)
        // builder.Plugins.AddFromObject(new AssignmentPlugin(assignmentRepository, courseRepository, canvasService, configurationService));
        
        // Build the kernel
        _kernel = builder.Build();
        
        _chatHistory = new List<ChatMessage>();
        _skChatHistory = new ChatHistory();
        
        // Add a system message
        var systemMessage = "You are a helpful AI assistant for a Canvas Calendar app. You have access to calculator functions. " +
            "When users ask you to add or multiply numbers, use the available functions to calculate the result. " +
            "Be friendly and let users know when you're using the calculator functions.";
        
        _chatHistory.Add(new ChatMessage(ChatRole.System, systemMessage));
        _skChatHistory.AddSystemMessage(systemMessage);
    }

    public async Task<string> SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return string.Empty;

        // Add user message to both chat histories
        _chatHistory.Add(new ChatMessage(ChatRole.User, message));
        _skChatHistory.AddUserMessage(message);

        try
        {
            // Create chat completion settings to enable function calling
            var settings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            // Get response from Semantic Kernel with function calling
            var response = await _kernel.GetRequiredService<IChatCompletionService>()
                .GetChatMessageContentAsync(_skChatHistory, settings, _kernel);

            var assistantMessage = response.Content ?? "";

            // Add assistant response to both chat histories
            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, assistantMessage));
            _skChatHistory.AddAssistantMessage(assistantMessage);

            return assistantMessage;
        }
        catch (Exception ex)
        {
            var error = $"Error: {ex.Message}";
            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, error));
            _skChatHistory.AddAssistantMessage(error);
            return error;
        }
    }

    public async IAsyncEnumerable<string> SendMessageStreamAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            yield break;

        // Add user message to both chat histories
        _chatHistory.Add(new ChatMessage(ChatRole.User, message));
        _skChatHistory.AddUserMessage(message);

        var assistantResponse = "";
        var chunks = new List<string>();
        
        try
        {
            // Create chat completion settings to enable function calling
            var settings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            // Get streaming response from Semantic Kernel
            await foreach (var update in _kernel.GetRequiredService<IChatCompletionService>()
                .GetStreamingChatMessageContentsAsync(_skChatHistory, settings, _kernel))
            {
                var chunk = update.Content ?? "";
                assistantResponse += chunk;
                chunks.Add(chunk);
            }

            // Add assistant response to both chat histories
            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, assistantResponse));
            _skChatHistory.AddAssistantMessage(assistantResponse);
        }
        catch (Exception ex)
        {
            var error = $"Error: {ex.Message}";
            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, error));
            _skChatHistory.AddAssistantMessage(error);
            chunks.Clear();
            chunks.Add(error);
        }

        // Yield chunks outside try-catch
        foreach (var chunk in chunks)
        {
            yield return chunk;
        }
    }

    public void ClearHistory()
    {
        var systemMessage = _chatHistory.FirstOrDefault(m => m.Role == ChatRole.System);
        _chatHistory.Clear();
        _skChatHistory.Clear();
        
        if (systemMessage != null)
        {
            _chatHistory.Add(systemMessage);
            _skChatHistory.AddSystemMessage(systemMessage.Text ?? "");
        }
    }

    public IReadOnlyList<ChatMessage> GetHistory()
    {
        return _chatHistory.AsReadOnly();
    }
}
