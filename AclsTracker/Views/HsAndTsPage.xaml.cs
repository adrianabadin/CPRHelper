using AclsTracker.ViewModels;
namespace AclsTracker.Views;
public partial class HsAndTsPage : ContentPage
{
    public HsAndTsPage(EventRecordingViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
