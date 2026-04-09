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
        
        // Restore session on startup using fire-and-forget to avoid deadlock risk
        // SetPersistence was already called in MauiProgram before InitializeAsync
        Task.Run(async () =>
        {
            try
            {
                await _supabase.InitializeAsync();
                await _supabase.Auth.RetrieveSessionAsync();
                System.Diagnostics.Debug.WriteLine("[App] Supabase session restored");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[App] Failed to restore Supabase session: {ex.Message}");
            }
        });
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var shell = _serviceProvider.GetRequiredService<AppShell>();
        return new Window(shell);
    }
}