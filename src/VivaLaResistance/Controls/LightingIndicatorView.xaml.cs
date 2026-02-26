using VivaLaResistance.Core.Interfaces;

namespace VivaLaResistance.Controls;

/// <summary>
/// Displays a lighting quality warning banner overlaid on the camera view.
/// Bind <see cref="Quality"/> to update the indicator from a ViewModel.
/// </summary>
public partial class LightingIndicatorView : Grid
{
    public static readonly BindableProperty QualityProperty =
        BindableProperty.Create(
            nameof(Quality),
            typeof(LightingQuality),
            typeof(LightingIndicatorView),
            LightingQuality.Good,
            propertyChanged: OnQualityChanged);

    public LightingQuality Quality
    {
        get => (LightingQuality)GetValue(QualityProperty);
        set => SetValue(QualityProperty, value);
    }

    /// <summary>True when a warning banner should be shown.</summary>
    public bool IsWarningVisible => Quality != LightingQuality.Good && Quality != LightingQuality.Unknown;

    public LightingIndicatorView()
    {
        InitializeComponent();
    }

    private static void OnQualityChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var view = (LightingIndicatorView)bindable;
        var quality = (LightingQuality)newValue;

        view.WarningLabel.Text = quality switch
        {
            LightingQuality.TooDark => "⚠️ Too dark — move to better light",
            LightingQuality.TooBright => "⚠️ Too bright — avoid direct light",
            _ => string.Empty
        };

        view.OnPropertyChanged(nameof(IsWarningVisible));
    }
}
