using Admin.ViewModels;

namespace Admin.Views;

public partial class CarsPage : ContentPage
{
    public CarsPage(CarsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is CarsViewModel vm)
            vm.LoadCarsCommand.Execute(null);
    }
}
