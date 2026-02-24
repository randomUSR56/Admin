using Admin.ViewModels;

namespace Admin.Views;

public partial class TicketDetailPage : ContentPage
{
    public TicketDetailPage(TicketDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
