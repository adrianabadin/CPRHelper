using AclsTracker.Services.EventLog;
namespace AclsTracker.Views;
public partial class HistorialPage : ContentPage
{
    public HistorialPage(IEventLogService eventLogService)
    {
        InitializeComponent();
        BindingContext = eventLogService;
    }
}
