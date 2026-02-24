using Admin.ViewModels;

namespace Admin.Views;

public partial class ProblemDetailPage : ContentPage
{
    public ProblemDetailPage(ProblemDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
