using Admin.ViewModels;

namespace Admin.Views;

public partial class CarDetailPage : ContentPage
{
    public CarDetailPage(CarDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}
