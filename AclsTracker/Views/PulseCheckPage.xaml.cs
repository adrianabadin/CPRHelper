using System.Collections.ObjectModel;

namespace AclsTracker.Views;

public partial class PulseCheckPage : ContentPage
{
    private readonly Action _onChequeandoPulso;
    private bool _pulseCheckStarted;
    public ObservableCollection<string> Suggestions { get; }

    public PulseCheckPage(List<string> suggestions, Action onChequeandoPulso)
    {
        _onChequeandoPulso = onChequeandoPulso;
        Suggestions = new ObservableCollection<string>(suggestions);
        InitializeComponent();
        BindingContext = this;
    }

    private void OnChequeandoClicked(object sender, EventArgs e)
    {
        if (!_pulseCheckStarted)
        {
            _pulseCheckStarted = true;
            _onChequeandoPulso(); // triggers pause in MainViewModel
            if (sender is Button btn)
            {
                btn.Text = "CHEQUEANDO...";
                btn.IsEnabled = false;
                btn.BackgroundColor = Colors.Gray;
            }
        }
    }

    private async void OnCerrarClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        return true; // Prevent back gesture from dismissing
    }
}
