using CanvasCalendar.PageModels;

namespace CanvasCalendar.Pages;

public partial class AssignmentListPage : ContentPage
{
    public AssignmentListPage(AssignmentListPageModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext is AssignmentListPageModel viewModel)
        {
            await viewModel.AppearingCommand.ExecuteAsync(null);
        }
    }
}
