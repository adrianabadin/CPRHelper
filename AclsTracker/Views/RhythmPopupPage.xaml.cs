namespace AclsTracker.Views;

public partial class RhythmPopupPage : ContentPage
{
    public RhythmPopupPage(string title, string message)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        MessageLabel.Text = message;
    }

    private async void OnContinuarClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    protected override bool OnBackButtonPressed()
    {
        return true; // Prevent back gesture from dismissing
    }
}
