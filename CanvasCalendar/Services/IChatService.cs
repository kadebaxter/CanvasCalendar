using Microsoft.Extensions.AI;

namespace CanvasCalendar.Services;

/// <summary>
/// Service interface for AI chat functionality using Ollama.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Sends a message to the AI chat and returns the response.
    /// </summary>
    /// <param name="message">The user's message</param>
    /// <returns>The AI's response</returns>
    Task<string> SendMessageAsync(string message);
    
    /// <summary>
    /// Sends a message to the AI chat and streams the response.
    /// </summary>
    /// <param name="message">The user's message</param>
    /// <returns>An async enumerable of response chunks</returns>
    IAsyncEnumerable<string> SendMessageStreamAsync(string message);
    
    /// <summary>
    /// Clears the conversation history.
    /// </summary>
    void ClearHistory();
    
    /// <summary>
    /// Gets the current conversation history.
    /// </summary>
    /// <returns>List of chat messages</returns>
    IReadOnlyList<ChatMessage> GetHistory();
}
