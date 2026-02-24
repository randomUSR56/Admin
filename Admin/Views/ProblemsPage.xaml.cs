using Admin.ViewModels;

namespace Admin.Views;

public partial class ProblemsPage : ContentPage
{
    private readonly ProblemsViewModel _viewModel;

    public ProblemsPage(ProblemsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadProblemsCommand.Execute(null);
    }
}
