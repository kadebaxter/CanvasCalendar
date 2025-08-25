using CanvasCalendar.PageModels;

namespace CanvasCalendar.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
