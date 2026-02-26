namespace VivaLaResistance.Views;

/// <summary>
/// Dismissible support prompt shown on each launch after the 7-day trial expires.
/// This is NOT a paywall — the full app remains accessible after dismissal.
/// </summary>
public partial class SupportModalPage : ContentPage
{
    public SupportModalPage()
    {
        InitializeComponent();
    }

    protected override bool OnBackButtonPressed()
    {
        // Android back button dismisses the modal
        _ = Navigation.PopModalAsync();
        return true;
    }

    private void OnBackgroundTapped(object? sender, TappedEventArgs e)
    {
        _ = Navigation.PopModalAsync();
    }

    private void OnCardTapped(object? sender, TappedEventArgs e)
    {
        // Swallow tap so it doesn't bubble to the background dismiss handler
    }

    private void OnDismissTapped(object? sender, TappedEventArgs e)
    {
        _ = Navigation.PopModalAsync();
    }

    private async void OnSupportTapped(object? sender, TappedEventArgs e)
    {
        // TODO: Replace with real App Store / Play Store product URL once published
        var storeUri = DeviceInfo.Platform == DevicePlatform.iOS
            ? new Uri("https://apps.apple.com/app/idTODO")   // TODO: replace with real Apple App ID
            : new Uri("https://play.google.com/store/apps/details?id=com.vivalaresistance.TODO");  // TODO: replace with real package ID

        if (await Launcher.CanOpenAsync(storeUri))
            await Launcher.OpenAsync(storeUri);

        // Dismiss modal after opening store
        await Navigation.PopModalAsync();
    }
}
