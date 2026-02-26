using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Core.Models;
using VivaLaResistance.ViewModels;

namespace VivaLaResistance;

/// <summary>
/// Main page with camera view for resistor detection.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;
    private readonly IFrameSource? _frameSource;

    /// <param name="frameSource">Nullable — will be null until Bruce's platform IFrameSource is registered in DI.</param>
    public MainPage(MainViewModel viewModel, IFrameSource? frameSource = null)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _frameSource = frameSource;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.IsCameraNotReady = true;
        _viewModel.IsPermissionDenied = false;

        await _viewModel.InitializeAsync();

        if (_frameSource is null)
        {
            _viewModel.StatusText = "Camera not available";
            return;
        }

        var granted = await RequestCameraPermissionAsync();
        if (!granted)
        {
            _viewModel.IsPermissionDenied = true;
            return;
        }

        _frameSource.FrameAvailable += OnFrameAvailable;
        await _frameSource.StartAsync();
        _viewModel.IsCameraNotReady = false;
        _viewModel.StatusText = "Ready - Point at a resistor";
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        if (_frameSource is not null && _frameSource.IsRunning)
        {
            _frameSource.FrameAvailable -= OnFrameAvailable;
            await _frameSource.StopAsync();
        }

        _viewModel.IsCameraNotReady = true;
        _viewModel.Cleanup();
    }

    private void OnFrameAvailable(object? sender, CameraFrame frame)
    {
        MainThread.BeginInvokeOnMainThread(async () =>
            await _viewModel.ProcessFrameAsync(frame.Data, frame.Width, frame.Height));
    }

    private void OnOpenSettingsClicked(object? sender, EventArgs e)
    {
        AppInfo.ShowSettingsUI();
    }

    private async Task<bool> RequestCameraPermissionAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status == PermissionStatus.Granted) return true;

        if (Permissions.ShouldShowRationale<Permissions.Camera>())
        {
            await DisplayAlertAsync(
                "Camera Required",
                "This app uses the camera to detect and identify resistors in real time.",
                "OK");
        }

        status = await Permissions.RequestAsync<Permissions.Camera>();
        if (status == PermissionStatus.Denied)
        {
            bool openSettings = await DisplayAlertAsync(
                "Camera Required",
                "Camera permission is required to detect resistors. Open Settings to enable it.",
                "Open Settings", "Cancel");
            if (openSettings)
                AppInfo.ShowSettingsUI();
            return false;
        }

        return status == PermissionStatus.Granted;
    }
}
