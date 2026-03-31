using CommunityToolkit.Maui;
using Plugin.Maui.Audio;
using QuestPDF.Infrastructure;
using AclsTracker.Services.Timer;
using AclsTracker.Services.Audio;
using AclsTracker.Services.EventLog;
using AclsTracker.Services.Database;
using AclsTracker.Services.Export;
using AclsTracker.Services.Auth;
using AclsTracker.Constants;
using AclsTracker.ViewModels;
using AclsTracker.Views;
using Supabase;

namespace AclsTracker;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Audio
        builder.Services.AddSingleton(AudioManager.Current);

        // ============ Supabase Auth Setup ============
        // Create session handler for persistence
        var sessionHandler = new SupabaseSessionHandler();
        
        // Create Supabase client with session persistence
        var supabase = new Client(SupabaseConfig.Url, SupabaseConfig.AnonKey, new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        });
        
        // Set persistence BEFORE any initialization calls
        // This ensures session tokens are saved/loaded from SecureStorage
        supabase.Auth.SetPersistence(sessionHandler);
        
        // Register Supabase client as singleton
        builder.Services.AddSingleton(supabase);
        
        // Register AuthService
        builder.Services.AddSingleton<IAuthService, AuthService>();
        // ===========================================

        // Services — implementations registered in Plans 02 and 03
        builder.Services.AddSingleton<IAudioService, AudioService>();
        builder.Services.AddSingleton<IMetronomeService, MetronomeService>();
        builder.Services.AddSingleton<ITimerService, TimerService>();
        builder.Services.AddSingleton<IEventLogService, EventLogService>();
        builder.Services.AddSingleton<ISessionRepository, SessionRepository>();

        // Export services — PDF and CSV generation
        builder.Services.AddSingleton<IPdfExportService, PdfExportService>();
        builder.Services.AddSingleton<ICsvExportService, CsvExportService>();

        // ViewModels — registered in Plans 02 and 03
        builder.Services.AddTransient<MetronomeViewModel>();
        builder.Services.AddTransient<TimerViewModel>();
        builder.Services.AddSingleton<EventRecordingViewModel>();
        builder.Services.AddSingleton<HistorialViewModel>();
        builder.Services.AddSingleton<AuthViewModel>();

        // Views
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<HsAndTsPage>();
        builder.Services.AddTransient<HistorialPage>();
        builder.Services.AddTransient<PatientDataPopup>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ProfilePage>();

        // QuestPDF community license (required before generating PDFs)
        QuestPDF.Settings.License = LicenseType.Community;

        return builder.Build();
    }
}