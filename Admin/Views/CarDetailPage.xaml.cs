using Admin.ViewModels;

namespace Admin.Views;

public partial class CarDetailPage : ContentPage
{
    public CarDetailPage(CarDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
