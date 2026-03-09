using VivaLaResistance.ViewModels;
using VivaLaResistance.Views;

namespace VivaLaResistance;

/// <summary>
/// Main page with camera view for resistor detection.
/// </summary>
public partial class MainPage : ContentPage
{
    private readonly MainViewModel _viewModel;

    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await InitializeCameraAsync();

        if (_viewModel.ShouldShowSupportModal)
        {
            await Navigation.PushModalAsync(new SupportModalPage(), animated: true);
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.Cleanup();
    }

    /// <summary>
    /// Initializes the camera with permission flow.
    /// </summary>
    private async Task InitializeCameraAsync()
    {
        // Reset state
        _viewModel.IsPermissionDenied = false;
        _viewModel.IsCameraNotReady = true;

        // Request camera permission
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        
        if (status != PermissionStatus.Granted)
        {
            // Show rationale if needed
            if (Permissions.ShouldShowRationale<Permissions.Camera>())
            {
                await DisplayAlertAsync(
                    "Camera Access",
                    "VivaLaResistance needs camera access to detect resistors.",
                    "OK");
            }

            status = await Permissions.RequestAsync<Permissions.Camera>();
        }

        if (status != PermissionStatus.Granted)
        {
            // Permission denied - show denied state
            _viewModel.IsPermissionDenied = true;
            _viewModel.IsCameraNotReady = true;
            return;
        }

        // Permission granted - initialize detection service and camera
        await _viewModel.InitializeAsync();
    }

    /// <summary>
    /// Handles the Open Settings button click when camera permission is denied.
    /// </summary>
    private void OnOpenSettingsClicked(object sender, EventArgs e)
    {
        AppInfo.ShowSettingsUI();
    }

    private void OnDismissErrorClicked(object? sender, EventArgs e)
    {
        if (BindingContext is MainViewModel vm)
            vm.ClearCameraError();
    }
}