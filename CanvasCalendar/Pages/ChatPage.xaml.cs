using CanvasCalendar.PageModels;

namespace CanvasCalendar.Pages;

/// <summary>
/// Chat page for AI assistant interaction.
/// </summary>
public partial class ChatPage : ContentPage
{
    private readonly ChatPageModel _viewModel;

    public ChatPage(ChatPageModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        
        // Subscribe to message added event for auto-scrolling
        _viewModel.MessageAdded += OnMessageAdded;
    }

    private void OnMessageAdded(object? sender, ChatMessageDisplay message)
    {
        // Auto-scroll to the latest message
        MessagesCollectionView.ScrollTo(message, position: ScrollToPosition.End, animate: true);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        
        // Unsubscribe to prevent memory leaks
        if (_viewModel != null)
        {
            _viewModel.MessageAdded -= OnMessageAdded;
        }
    }
}
