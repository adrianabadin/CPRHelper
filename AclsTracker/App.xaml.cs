using Supabase;

namespace AclsTracker;

public partial class App : Application
{
    private readonly Client _supabase;

    public App(Client supabase)
    {
        _supabase = supabase;
        
        InitializeComponent();
        
        // Restore session on startup using fire-and-forget to avoid deadlock risk
        // SetPersistence was already called in MauiProgram before InitializeAsync
        Task.Run(async () =>
        {
            try
            {
                await _supabase.InitializeAsync();
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
        return new Window(new AppShell());
    }
}