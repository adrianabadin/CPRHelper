using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace AclsTracker.Controls;

/// <summary>
/// Reusable notification banner with auto-dismiss and close button.
/// Shows at the top of the page, auto-hides after a configurable duration.
/// </summary>
public partial class NotificationBanner : ContentView, INotifyPropertyChanged
{
    private IDispatcherTimer? _dismissTimer;
    private string _bannerMessage = string.Empty;
    private bool _isBannerVisible;

    public new event PropertyChangedEventHandler? PropertyChanged;

    public string BannerMessage
    {
        get => _bannerMessage;
        set { _bannerMessage = value; OnPropertyChanged(); }
    }

    public bool IsBannerVisible
    {
        get => _isBannerVisible;
        set { _isBannerVisible = value; OnPropertyChanged(); }
    }

    public Command DismissCommand { get; }

    public NotificationBanner()
    {
        DismissCommand = new Command(HideBanner);
        InitializeComponent();
        BindingContext = this;
    }

    /// <summary>
    /// Show the banner with a message that auto-dismisses after the specified duration.
    /// </summary>
    public void Show(string message, int autoDismissSeconds = 5)
    {
        BannerMessage = message;
        IsBannerVisible = true;

        _dismissTimer?.Stop();
        _dismissTimer = Application.Current!.Dispatcher.CreateTimer();
        _dismissTimer.Interval = TimeSpan.FromSeconds(autoDismissSeconds);
        _dismissTimer.IsRepeating = false;
        _dismissTimer.Tick += (_, _) =>
        {
            _dismissTimer.Stop();
            HideBanner();
        };
        _dismissTimer.Start();
    }

    /// <summary>
    /// Hide the banner immediately.
    /// </summary>
    public void HideBanner()
    {
        IsBannerVisible = false;
        _dismissTimer?.Stop();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
