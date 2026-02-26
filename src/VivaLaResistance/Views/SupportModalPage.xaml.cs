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
}
