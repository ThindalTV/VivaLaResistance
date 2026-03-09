using VivaLaResistance.Core.Interfaces;

namespace VivaLaResistance;

/// <summary>
/// MAUI Application class.
/// </summary>
public partial class App : Application
{
    private readonly IFrameSource? _frameSource;

    public App(IFrameSource? frameSource = null)
    {
        InitializeComponent();
        _frameSource = frameSource;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    /// <summary>
    /// Called when the app moves to the background (home button, incoming call, lock screen).
    /// Stop the camera to conserve battery and release the camera hardware.
    /// The camera will restart via MainPage.OnAppearing when the app resumes.
    /// </summary>
    protected override void OnSleep()
    {
        base.OnSleep();

        if (_frameSource is not null && _frameSource.IsRunning)
        {
            // Fire-and-forget stop — app lifecycle callbacks must be synchronous.
            _ = _frameSource.StopAsync();
        }
    }
}
