using Admin.ViewModels;

namespace Admin.Views;

public partial class ProblemDetailPage : ContentPage
{
    public ProblemDetailPage(ProblemDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
