using VivaLaResistance.Core.Interfaces;

namespace VivaLaResistance;

/// <summary>
/// MAUI Application class. Manages app-level lifecycle to release camera and
/// pause ML inference when the app is backgrounded.
/// </summary>
public partial class App : Application
{
    private readonly IFrameSource _frameSource;
    private readonly IResistorDetectionService _detectionService;
    private bool _cameraWasRunning;

    public App(IFrameSource frameSource, IResistorDetectionService detectionService)
    {
        _frameSource = frameSource;
        _detectionService = detectionService;
        InitializeComponent();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());

        window.Stopped += OnWindowStopped;
        window.Resumed += OnWindowResumed;
        window.Destroying += OnWindowDestroying;

        return window;
    }

    private void OnWindowStopped(object? sender, EventArgs e)
    {
        // App going to background - release camera and pause inference
        _cameraWasRunning = _frameSource.IsRunning;
        if (_cameraWasRunning)
        {
            _ = _frameSource.StopAsync();
        }
        _detectionService.Pause();
    }

    private void OnWindowResumed(object? sender, EventArgs e)
    {
        // App returning to foreground - resume inference and restart camera if needed
        _detectionService.Resume();
        if (_cameraWasRunning)
        {
            _cameraWasRunning = false;
            _ = _frameSource.StartAsync();
        }
    }

    private void OnWindowDestroying(object? sender, EventArgs e)
    {
        if (_frameSource.IsRunning)
        {
            _ = _frameSource.StopAsync();
        }
        _detectionService.Pause();
    }
}