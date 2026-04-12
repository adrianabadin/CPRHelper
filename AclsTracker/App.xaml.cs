using Supabase;

namespace AclsTracker;

public partial class App : Application
{
    private readonly Client _supabase;
    private readonly IServiceProvider _serviceProvider;

    public App(IServiceProvider serviceProvider, Client supabase)
    {
        _serviceProvider = serviceProvider;
        _supabase = supabase;

        InitializeComponent();

        // LoadSession() already hydrated the session in MauiProgram.
        // Refresh token in background — blocking the UI thread with GetAwaiter().GetResult()
        // causes a deadlock on Android because HTTP callbacks need the UI thread.
        _ = RefreshSessionAsync();
    }

    private async Task RefreshSessionAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"[App] Before InitializeAsync, CurrentSession: {_supabase.Auth.CurrentSession != null}");
            await _supabase.InitializeAsync();
            System.Diagnostics.Debug.WriteLine($"[App] After InitializeAsync, CurrentSession: {_supabase.Auth.CurrentSession != null}");
            await _supabase.Auth.RetrieveSessionAsync();
            System.Diagnostics.Debug.WriteLine($"[App] After RetrieveSessionAsync, CurrentSession: {_supabase.Auth.CurrentSession != null}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Session restore failed: {ex.Message}");
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = _serviceProvider.GetRequiredService<AppShell>();
        return new Window(shell);
    }
}