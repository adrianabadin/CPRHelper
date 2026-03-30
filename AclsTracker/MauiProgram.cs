using CommunityToolkit.Maui;
using Plugin.Maui.Audio;
using AclsTracker.Services.Timer;
using AclsTracker.Services.Audio;
using AclsTracker.Services.EventLog;
using AclsTracker.Services.Database;
using AclsTracker.ViewModels;
using AclsTracker.Views;

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

        // Services — implementations registered in Plans 02 and 03
        builder.Services.AddSingleton<IAudioService, AudioService>();
        builder.Services.AddSingleton<IMetronomeService, MetronomeService>();
        builder.Services.AddSingleton<ITimerService, TimerService>();
        builder.Services.AddSingleton<IEventLogService, EventLogService>();
        builder.Services.AddSingleton<ISessionRepository, SessionRepository>();

        // ViewModels — registered in Plans 02 and 03
        builder.Services.AddTransient<MetronomeViewModel>();
        builder.Services.AddTransient<TimerViewModel>();
        builder.Services.AddSingleton<EventRecordingViewModel>();

        // Views
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();
        builder.Services.AddTransient<HsAndTsPage>();
        builder.Services.AddTransient<HistorialPage>();
        builder.Services.AddTransient<PatientDataPopup>();

        return builder.Build();
    }
}