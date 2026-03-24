using CommunityToolkit.Maui;
using Plugin.Maui.Audio;
using AclsTracker.Services.Timer;
using AclsTracker.Services.Audio;
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

        // ViewModels — registered in Plans 02 and 03
        builder.Services.AddTransient<MetronomeViewModel>();
        builder.Services.AddTransient<TimerViewModel>();

        // Views — registered in Plan 04
        builder.Services.AddTransient<MainViewModel>();
        builder.Services.AddTransient<MainPage>();

        return builder.Build();
    }
}