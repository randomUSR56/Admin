using Admin.ViewModels;

namespace Admin.Views;

public partial class TicketDetailPage : ContentPage
{
    public TicketDetailPage(TicketDetailViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
