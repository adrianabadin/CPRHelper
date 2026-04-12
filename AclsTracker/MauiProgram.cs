using CommunityToolkit.Maui;
using Plugin.Maui.Audio;
#if !ANDROID && !IOS
using QuestPDF.Infrastructure;
#endif
using AclsTracker.Services.Timer;
using AclsTracker.Services.Audio;
using AclsTracker.Services.EventLog;
using AclsTracker.Services.Database;
using AclsTracker.Services.Export;
using AclsTracker.Services.Auth;
using AclsTracker.Services.Sync;
using AclsTracker.Constants;
using AclsTracker.ViewModels;
using AclsTracker.Views;
using Supabase;
using Microsoft.Extensions.Configuration;
using System.Reflection;

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

        // ============ Configuration ============
        // User Secrets only work on Windows desktop. For Android/iOS, config is
        // embedded as a resource at build time (appsettings.json, gitignored).
        var assembly = Assembly.GetExecutingAssembly();
        var stream = assembly.GetManifestResourceStream("AclsTracker.appsettings.json");

        var configBuilder = new ConfigurationBuilder();
        if (stream != null)
            configBuilder.AddJsonStream(stream);

        var config = configBuilder
            .AddUserSecrets(assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();
        
        // Pass configuration to SupabaseConfig
        SupabaseConfig.Initialize(config);

        // Audio
        builder.Services.AddSingleton(AudioManager.Current);

        // ============ Supabase Auth Setup ============
        var sessionHandler = new SupabaseSessionHandler();
        
        var supabase = new Client(SupabaseConfig.Url, SupabaseConfig.AnonKey, new SupabaseOptions
        {
            AutoRefreshToken = true,
            AutoConnectRealtime = false
        });
        
        supabase.Auth.SetPersistence(sessionHandler);
        supabase.Auth.LoadSession();
        System.Diagnostics.Debug.WriteLine($"[MauiProgram] LoadSession complete, CurrentSession is {(supabase.Auth.CurrentSession != null ? "present" : "null")}");

        builder.Services.AddSingleton(supabase);
        
        builder.Services.AddSingleton<IAuthService, AuthService>();
        // ===========================================

        // Services
        builder.Services.AddSingleton<IAudioService, AudioService>();
        builder.Services.AddSingleton<IMetronomeService, MetronomeService>();
        builder.Services.AddSingleton<ITimerService, TimerService>();
        builder.Services.AddSingleton<IEventLogService, EventLogService>();
        builder.Services.AddSingleton<ISessionRepository, SessionRepository>();
        builder.Services.AddSingleton<ISessionSyncService, SessionSyncService>();

        // Export services
        builder.Services.AddSingleton<IPdfExportService, PdfExportService>();
        builder.Services.AddSingleton<ICsvExportService, CsvExportService>();

        // ViewModels
        builder.Services.AddTransient<MetronomeViewModel>();
        builder.Services.AddTransient<TimerViewModel>();
        builder.Services.AddSingleton<EventRecordingViewModel>();
        builder.Services.AddSingleton<HistorialViewModel>();
        builder.Services.AddSingleton<AuthViewModel>();

        // Views
        builder.Services.AddSingleton<AppShell>();
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<HsAndTsPage>();
        builder.Services.AddTransient<HistorialPage>();
        builder.Services.AddTransient<PatientDataPopup>();
        builder.Services.AddTransient<LoginPage>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<ProfilePage>();

        // QuestPDF community license (desktop only - not supported on Android/iOS)
#if !ANDROID && !IOS
        QuestPDF.Settings.License = LicenseType.Community;
#endif

        return builder.Build();
    }
}