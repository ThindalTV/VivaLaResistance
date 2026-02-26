using Microsoft.Extensions.Logging;
using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Services;
using VivaLaResistance.ViewModels;

namespace VivaLaResistance;

/// <summary>
/// MAUI application entry point and dependency injection configuration.
/// </summary>
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Register services
        RegisterServices(builder.Services);

        // Register ViewModels
        RegisterViewModels(builder.Services);

        // Register Pages
        RegisterPages(builder.Services);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }

    private static void RegisterServices(IServiceCollection services)
    {
        // Core services - Singleton for app lifetime
        services.AddSingleton<IResistorValueCalculatorService, ResistorValueCalculatorService>();
        services.AddSingleton<IResistorDetectionService, ResistorDetectionService>();
        services.AddSingleton<ITrialService, TrialService>();
        // Rhodes gate: pending review before merge
        services.AddSingleton<IResistorLocalizationService, OnnxResistorLocalizationService>();

        // Infrastructure services
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IPreferencesWrapper, MauiPreferencesWrapper>();
    }

    private static void RegisterViewModels(IServiceCollection services)
    {
        services.AddTransient<MainViewModel>();
    }

    private static void RegisterPages(IServiceCollection services)
    {
        services.AddTransient<MainPage>();
    }
}

/// <summary>
/// MAUI Preferences wrapper implementation using Microsoft.Maui.Storage.Preferences.
/// </summary>
public class MauiPreferencesWrapper : IPreferencesWrapper
{
    /// <inheritdoc />
    public T? Get<T>(string key, T? defaultValue)
    {
        if (typeof(T) == typeof(string))
        {
            var value = Preferences.Default.Get(key, defaultValue as string);
            return (T?)(object?)value;
        }

        // For other types, store as JSON or use appropriate Preferences overload
        var stringValue = Preferences.Default.Get(key, (string?)null);
        if (string.IsNullOrEmpty(stringValue))
        {
            return defaultValue;
        }

        // Simple handling for string type
        return (T?)(object?)stringValue;
    }

    /// <inheritdoc />
    public void Set<T>(string key, T value)
    {
        if (value is string stringValue)
        {
            Preferences.Default.Set(key, stringValue);
        }
        else
        {
            // For non-string types, convert to string
            Preferences.Default.Set(key, value?.ToString());
        }
    }

    /// <inheritdoc />
    public void Remove(string key)
    {
        Preferences.Default.Remove(key);
    }
}
