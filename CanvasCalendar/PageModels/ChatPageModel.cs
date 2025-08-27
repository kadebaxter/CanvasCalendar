using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CanvasCalendar.Services;
using System.Collections.ObjectModel;
using Microsoft.Extensions.AI;

namespace CanvasCalendar.PageModels;

/// <summary>
/// Page model for the chat interface.
/// </summary>
public partial class ChatPageModel : ObservableObject
{
    private readonly IChatService _chatService;

    [ObservableProperty]
    private string _messageText = string.Empty;

    [ObservableProperty]
    private bool _isSending = false;

    [ObservableProperty]
    private bool _isStreaming = false;

    /// <summary>
    /// Collection of chat messages for display.
    /// </summary>
    public ObservableCollection<ChatMessageDisplay> Messages { get; } = new();

    /// <summary>
    /// Event raised when a new message is added and we should scroll to it.
    /// </summary>
    public event EventHandler<ChatMessageDisplay>? MessageAdded;

    public ChatPageModel(IChatService chatService)
    {
        _chatService = chatService;
        
        // Add a welcome message
        var welcomeMessage = new ChatMessageDisplay
        {
            IsFromUser = false,
            Message = "Hello! I'm your AI assistant. I can help you with questions about your assignments, scheduling, and general academic planning. What would you like to know?",
            Timestamp = DateTime.Now
        };
        Messages.Add(welcomeMessage);
    }

    /// <summary>
    /// Command to send a message to the AI.
    /// </summary>
    [RelayCommand]
    private async Task SendMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(MessageText) || IsSending)
            return;

        var userMessage = MessageText.Trim();
        MessageText = string.Empty;
        IsSending = true;

        try
        {
            // Add user message to display
            var userMessageDisplay = new ChatMessageDisplay
            {
                IsFromUser = true,
                Message = userMessage,
                Timestamp = DateTime.Now
            };
            Messages.Add(userMessageDisplay);
            MessageAdded?.Invoke(this, userMessageDisplay);

            // Create placeholder for AI response
            var aiMessageDisplay = new ChatMessageDisplay
            {
                IsFromUser = false,
                Message = "",
                Timestamp = DateTime.Now,
                IsTyping = true
            };
            Messages.Add(aiMessageDisplay);
            MessageAdded?.Invoke(this, aiMessageDisplay);

            // Use streaming for better UX
            IsStreaming = true;
            await foreach (var chunk in _chatService.SendMessageStreamAsync(userMessage))
            {
                aiMessageDisplay.Message += chunk;
            }
            
            aiMessageDisplay.IsTyping = false;
        }
        catch (Exception ex)
        {
            // Remove the typing indicator if it exists
            var lastMessage = Messages.LastOrDefault();
            if (lastMessage?.IsTyping == true)
            {
                Messages.Remove(lastMessage);
            }

            // Add error message
            var errorMessage = new ChatMessageDisplay
            {
                IsFromUser = false,
                Message = $"Sorry, I encountered an error: {ex.Message}",
                Timestamp = DateTime.Now,
                IsError = true
            };
            Messages.Add(errorMessage);
            MessageAdded?.Invoke(this, errorMessage);
        }
        finally
        {
            IsSending = false;
            IsStreaming = false;
        }
    }

    /// <summary>
    /// Command to clear the chat history.
    /// </summary>
    [RelayCommand]
    private void ClearChat()
    {
        Messages.Clear();
        _chatService.ClearHistory();
        
        // Add welcome message back
        var welcomeMessage = new ChatMessageDisplay
        {
            IsFromUser = false,
            Message = "Chat cleared! How can I help you?",
            Timestamp = DateTime.Now
        };
        Messages.Add(welcomeMessage);
        MessageAdded?.Invoke(this, welcomeMessage);
    }
}

/// <summary>
/// Display model for chat messages.
/// </summary>
public partial class ChatMessageDisplay : ObservableObject
{
    [ObservableProperty]
    private bool _isFromUser;

    [ObservableProperty]
    private string _message = string.Empty;

    [ObservableProperty]
    private DateTime _timestamp;

    [ObservableProperty]
    private bool _isTyping;

    [ObservableProperty]
    private bool _isError;
}
