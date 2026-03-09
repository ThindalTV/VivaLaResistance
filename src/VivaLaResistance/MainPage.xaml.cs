using VivaLaResistance.Core.Interfaces;
using VivaLaResistance.Core.Models;
using VivaLaResistance.ViewModels;
using VivaLaResistance.Views;

namespace VivaLaResistance;

/// <summary>
/// Main page — full-screen camera view with AR overlay for resistor detection.
/// Handles IFrameSource lifecycle and feeds frames into MainViewModel.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly IFrameSource? _frameSource;

    public MainPage(MainViewModel viewModel, IFrameSource? frameSource = null)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _frameSource = frameSource;

        if (_frameSource is not null)
        {
            _frameSource.FrameAvailable += OnFrameAvailable;
            _frameSource.ErrorOccurred += OnCameraErrorOccurred;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is not MainViewModel vm)
            return;

        // Reset transient UI state for each appearance (handles app-resume)
        vm.IsCameraNotReady = true;
        vm.IsPermissionDenied = false;
        vm.ClearCameraError();

        await vm.InitializeAsync();

        if (vm.ShouldShowSupportModal)
            await Navigation.PushModalAsync(new SupportModalPage(), animated: true);

        await StartCameraAsync(vm);
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        await StopCameraAsync();

        if (BindingContext is MainViewModel vm)
            vm.Cleanup();
    }

    // ── Camera lifecycle ──────────────────────────────────────────────────────

    private async Task StartCameraAsync(MainViewModel vm)
    {
        if (_frameSource is null)
            return;

        // Request permission via MAUI Permissions API before starting hardware
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
        {
            // Show rationale on first request
            if (status == PermissionStatus.Unknown)
            {
                var rationale = await DisplayAlertAsync(
                    "Camera Access",
                    "Viva La Resistance needs your camera to detect resistor values in real time.",
                    "Allow",
                    "Deny");

                if (!rationale)
                {
                    vm.IsPermissionDenied = true;
                    vm.IsCameraNotReady = true;
                    return;
                }
            }

            status = await Permissions.RequestAsync<Permissions.Camera>();
        }

        if (status != PermissionStatus.Granted)
        {
            vm.IsPermissionDenied = true;
            vm.IsCameraNotReady = true;
            await DisplayAlertAsync(
                "Permission Required",
                "Camera access was denied. Open Settings to grant permission.",
                "Open Settings",
                "Cancel");
            AppInfo.ShowSettingsUI();
            return;
        }

        try
        {
            await _frameSource.StartAsync();
            vm.IsCameraNotReady = false;
            vm.StatusText = "Point at a resistor";
        }
        catch (Exception)
        {
            // Error is surfaced via ErrorOccurred event — no duplicate handling needed
            vm.IsCameraNotReady = true;
        }
    }

    private async Task StopCameraAsync()
    {
        if (_frameSource is null || !_frameSource.IsRunning)
            return;

        // Unsubscribe before stopping to prevent any last-frame races
        _frameSource.FrameAvailable -= OnFrameAvailable;
        _frameSource.ErrorOccurred -= OnCameraErrorOccurred;

        await _frameSource.StopAsync();

        // Re-subscribe so the page is ready the next time it appears
        _frameSource.FrameAvailable += OnFrameAvailable;
        _frameSource.ErrorOccurred += OnCameraErrorOccurred;
    }

    // ── Frame pipeline ────────────────────────────────────────────────────────

    private void OnFrameAvailable(object? sender, CameraFrame frame)
    {
        // Capture frames may arrive on a background capture thread; marshal to main thread.
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (BindingContext is MainViewModel vm)
                await vm.ProcessFrameAsync(frame.Data, frame.Width, frame.Height);
        });
    }

    private void OnCameraErrorOccurred(object? sender, Exception ex)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            if (BindingContext is MainViewModel vm)
                vm.OnCameraError(ex);
        });
    }

    // ── UI event handlers ─────────────────────────────────────────────────────

    private void OnOpenSettingsClicked(object? sender, EventArgs e)
    {
        AppInfo.ShowSettingsUI();
    }

    private void OnDismissErrorClicked(object? sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
            vm.ClearCameraError();
    }
}
