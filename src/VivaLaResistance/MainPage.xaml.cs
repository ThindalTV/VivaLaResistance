using VivaLaResistance.ViewModels;

namespace VivaLaResistance;

/// <summary>
/// Main page with camera view for resistor detection.
/// </summary>
public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is MainViewModel vm)
        {
            await vm.InitializeAsync();
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        if (BindingContext is MainViewModel vm)
        {
            vm.Cleanup();
        }
    }
}
