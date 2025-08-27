using Microsoft.Extensions.AI;
using OllamaSharp;

namespace CanvasCalendar.Services;

/// <summary>
/// Service implementation for AI chat functionality using Ollama.
/// </summary>
public class ChatService : IChatService
{
    private readonly IChatClient _chatClient;
    private readonly List<ChatMessage> _chatHistory;

    public ChatService()
    {
        // Initialize OllamaApiClient - exactly like the blog post
        _chatClient = new OllamaApiClient(new Uri("http://localhost:11434/"), "llama3.2:3b");
        _chatHistory = new List<ChatMessage>();
        
        // Add a system message
        _chatHistory.Add(new ChatMessage(ChatRole.System, 
            "You are a helpful AI assistant for a Canvas Calendar app. Be concise and friendly."));
    }

    public async Task<string> SendMessageAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return string.Empty;

        // Add user message to chat history
        _chatHistory.Add(new ChatMessage(ChatRole.User, message));

        try
        {
            // Stream the AI response exactly like the blog post
            var assistantResponse = "";
            await foreach (var update in _chatClient.GetStreamingResponseAsync(_chatHistory))
            {
                assistantResponse += update.Text;
            }

            // Append assistant message to chat history
            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, assistantResponse));

            return assistantResponse;
        }
        catch (Exception ex)
        {
            var error = $"Error: {ex.Message}";
            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, error));
            return error;
        }
    }

    public async IAsyncEnumerable<string> SendMessageStreamAsync(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            yield break;

        // Add user message to chat history
        _chatHistory.Add(new ChatMessage(ChatRole.User, message));

        var assistantResponse = "";
        var chunks = new List<string>();
        
        try
        {
            // Stream the AI response exactly like the blog post
            await foreach (var update in _chatClient.GetStreamingResponseAsync(_chatHistory))
            {
                assistantResponse += update.Text;
                chunks.Add(update.Text ?? "");
            }

            // Append assistant message to chat history
            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, assistantResponse));
        }
        catch (Exception ex)
        {
            var error = $"Error: {ex.Message}";
            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, error));
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
        if (systemMessage != null)
            _chatHistory.Add(systemMessage);
    }

    public IReadOnlyList<ChatMessage> GetHistory()
    {
        return _chatHistory.AsReadOnly();
    }
}
